// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Runtime
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureFunctionsV2.HttpExtensions.Annotations;
    using AzureFunctionsV2.HttpExtensions.Infrastructure;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Integrations.Withings.Domain;
    using H3.Integrations.Withings.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    /// <summary>
    /// This class implements all the endpoints required to integrate with the Withings IoMT API:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="WithingsAuth"/> implements the endpoint that clients must call to link a user's account
    ///     with their Withings account after the initial OAuth authorization has been performed in the Withings
    ///     system. Once the accounts are linked, the user can start ingesting historical and future data from
    ///     the Withings system into their account.
    ///     <a href="https://developer.withings.com/oauth2/#section/OAuth/OAuth-2.0">See documentation.</a>
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="WithingsCallback"/> implements the endpoint which Withings calls when new data is available for a specific user account and
    ///     <see cref="WithingsCallbackValidityCheck"/> implements an additional HTTP method on the same enpdoint which Withings calls to validate
    ///     that the callback endpoint is reachable and accepts requests.
    ///     The callback endpoint should only be called by the Withings system: this restriction is implemented via an IP-filter policy in Azure API Management.
    ///     The callback endpoint must return a HTTP 200 response within 2 seconds; failure to do so will lead Withings to retry
    ///     delivering the notification and repeated failure will remove the notification. As such, the endpoint is deliberately lean
    ///     and only perform light validation before enquing a message and delegating further processing to a background process.
    ///     <a href="https://developer.withings.com/oauth2/#section/DATA-API/Notifications">See documentation.</a>
    ///   </description></item>
    /// </list>
    /// </summary>
    [PublicApi]
    public class Apis
    {
        public const string CallbackUrl = "withings/callback";

        private readonly ILogger<Apis> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IAuthentication auth;
        private readonly IConsentStore consentStore;
        private readonly IQueue queue;
        private readonly IUserFactory userFactory;
        private readonly IVendorClient vendorClient;
        private readonly IWithingsClient withingsClient;
        private readonly IWithingsToFhirConverter withingsToFhirConverter;

        public Apis(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IAuthentication auth,
            IConsentStore consentStore,
            IQueue queue,
            IUserFactory userFactory,
            IVendorClient vendorClient,
            IWithingsClient withingsClient,
            IWithingsToFhirConverter withingsToFhirConverter)
        {
            this.log = log.CreateLogger<Apis>();
            this.queue = queue;
            this.userFactory = userFactory;
            this.vendorClient = vendorClient;
            this.withingsClient = withingsClient;
            this.withingsToFhirConverter = withingsToFhirConverter;
            this.exceptionFilter = exceptionFilter;
            this.auth = auth;
            this.consentStore = consentStore;
        }

        [SwaggerResponse(200, typeof(string), Description = "Ok")]
        [FunctionName(nameof(WithingsCallbackValidityCheck))]
        public JsonResult WithingsCallbackValidityCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "head", Route = CallbackUrl)] HttpRequest req)
        {
            return new JsonResult("Ok");
        }

        [SwaggerResponse(200, typeof(string), Description = "Ok")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(WithingsCallback))]
        public async Task<JsonResult> WithingsCallback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = CallbackUrl)] HttpRequest req,
            [HttpForm(Name = "userid")]HttpParam<int?> withingsUserId,
            [HttpForm(Name = "deviceid")]HttpParam<int?> withingsDeviceId,
            [HttpForm(Name = "appli")]HttpParam<int?> notificationCategory,
            [HttpForm(Name = "startdate")]HttpParam<long?> startDateEpoch,
            [HttpForm(Name = "enddate")]HttpParam<long?> endDateEpoch,
            [HttpForm(Name = "date")]HttpParam<string> dataOrEventDate,
            [HttpForm(Name = "action")]HttpParam<string> action)
        {
            if (withingsUserId.Value == null || notificationCategory.Value == null || startDateEpoch.Value == null || endDateEpoch.Value == null)
            {
                log.LogWarning("Skipping Withings notification with missing fields: userid={userid} appli={appli} startdate={startdate} enddate={enddate} deviceid={deviceid} date={date} action={action}", withingsUserId, notificationCategory, startDateEpoch, endDateEpoch, withingsDeviceId, dataOrEventDate, action);
                return new JsonResult("Missing fields");
            }

            log.LogInformation("Got Withings notification: userid={userid} appli={appli} startdate={startdate} enddate={enddate}", withingsUserId, notificationCategory, startDateEpoch, endDateEpoch);

            await queue.SendMessage(Jobs.NotificationQueueName, new CallbackReceivedMessage
            {
                WithingsUserId = withingsUserId.Value.Value,
                StartDateEpoch = startDateEpoch.Value.Value,
                EndDateEpoch = endDateEpoch.Value.Value,
            });

            return new JsonResult("Ok");
        }

        [SwaggerResponse(200, typeof(User), Description = "Linked Withings account")]
        [SwaggerResponse(400, typeof(string), Description = "Bad request")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(409, typeof(string), Description = "Conflict")]
        [SwaggerResponse(429, typeof(string), Description = "Too many requests")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(WithingsAuth))]
        public Task<JsonResult> WithingsAuth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "withings/auth")] HttpRequest req,
            [HttpBody(Required = true)] HttpParam<AuthDto> authDto,
            [HttpHeader(Name = "Authorization")] HttpParam<string> authorization,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var withingsAccessCode = authDto.Value.WithingsAccessCode;
                var withingsRedirectUri = authDto.Value.WithingsRedirectUri;

                var (userId, _, _) = await auth.ValidateUser(authorization, cancellationToken);
                var consent = await consentStore.FetchConsent(userId, cancellationToken);

                var newWithingsUserId = await withingsClient.CreateAccount(userId, withingsAccessCode, withingsRedirectUri, cancellationToken);

                if (consent == null || !consent.ExternalIds.TryGetValue(withingsToFhirConverter.System, out var withingsUserId) || withingsUserId != newWithingsUserId)
                {
                    consent ??= ConsentFactory.WithId(userId);
                    consent.ExternalIds[withingsToFhirConverter.System] = newWithingsUserId;
                    await consentStore.WriteConsent(consent, cancellationToken);
                }

                return new JsonResult(await userFactory.CreateUser(consent, jobId: null, cancellationToken));
            });
        }
    }
}
