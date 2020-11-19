// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Apis
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureFunctionsV2.HttpExtensions.Annotations;
    using AzureFunctionsV2.HttpExtensions.Infrastructure;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Core.Models.Fhir;
    using H3.Core.Runtime.Jobs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    /// <summary>
    /// This class implements endpoints to manage a user's account.
    /// <list type="bullet">
    ///   <item><description><see cref="GetUser"/> retrieves information about the user.</description></item>
    ///   <item><description><see cref="DeleteUser"/> irrevocably deletes a user's account.</description></item>
    ///   <item><description><see cref="CreateOrUpdateUser"/> manages the user's account, e.g. connecting or disconencting devices for various IoMT vendors.</description></item>
    /// </list>
    /// </summary>
    [PublicApi]
    public class UserEndpoint
    {
        private readonly ILogger<UserEndpoint> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IAuthentication auth;
        private readonly IConsentStore consentStore;
        private readonly IVendorClient vendorClient;
        private readonly IFhirClient fhirClient;
        private readonly IUserFactory userFactory;
        private readonly IGuidFactory guidFactory;

        public UserEndpoint(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IAuthentication auth,
            IConsentStore consentStore,
            IVendorClient vendorClient,
            IFhirClient fhirClient,
            IUserFactory userFactory,
            IGuidFactory guidFactory)
        {
            this.log = log.CreateLogger<UserEndpoint>();
            this.exceptionFilter = exceptionFilter;
            this.auth = auth;
            this.consentStore = consentStore;
            this.vendorClient = vendorClient;
            this.fhirClient = fhirClient;
            this.userFactory = userFactory;
            this.guidFactory = guidFactory;
        }

        [SwaggerResponse(200, typeof(User), Description = "User details")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(404, typeof(string), Description = "Not Found")]
        [SwaggerResponse(409, typeof(string), Description = "Conflict")]
        [SwaggerResponse(429, typeof(string), Description = "Too many requests")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(GetUser))]
        public Task<JsonResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequest req,
            [HttpHeader(Name = "Authorization")]HttpParam<string> authorization,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var (userId, _, _) = await auth.ValidateUser(authorization, cancellationToken);
                var consent = await consentStore.FetchConsent(userId, cancellationToken);
                consent = consent.Verify(userId, ignoreFhir: true);

                return new JsonResult(await userFactory.CreateUser(consent, jobId: null, cancellationToken));
            });
        }

        [SwaggerResponse(201, typeof(User), Description = "User will be deleted shortly")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(404, typeof(string), Description = "Not Found")]
        [SwaggerResponse(409, typeof(string), Description = "Conflict")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(DeleteUser))]
        public Task<JsonResult> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "user")] HttpRequest req,
            [HttpHeader(Name = "Authorization")]HttpParam<string> authorization,
            [DurableClient] IDurableOrchestrationClient jobClient,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var (userId, _, _) = await auth.ValidateUser(authorization, cancellationToken);

                var jobId = await jobClient.RunSingleton(
                    userId,
                    workflow: nameof(AccountDeletionJob.AccountDeletionWorkflow),
                    jobArguments: new StartAccountDeletionMessage
                    {
                        UserId = userId,
                    },
                    log,
                    guidFactory);

                return new JsonResult(new UserDeletion
                {
                    JobId = jobId,
                })
                {
                    StatusCode = (int?)HttpStatusCode.Accepted,
                };
            });
        }

        [SwaggerResponse(200, typeof(User), Description = "Updated user")]
        [SwaggerResponse(201, typeof(User), Description = "Ingestion of device data started")]
        [SwaggerResponse(204, typeof(User), Description = "No change was processed")]
        [SwaggerResponse(400, typeof(string), Description = "Bad request")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(409, typeof(string), Description = "Conflict")]
        [SwaggerResponse(429, typeof(string), Description = "Too many requests")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(CreateOrUpdateUser))]
        public Task<JsonResult> CreateOrUpdateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user")] HttpRequest req,
            [HttpBody(Required = true)] HttpParam<UserDto> userDto,
            [HttpHeader(Name = "Authorization")]HttpParam<string> authorization,
            [DurableClient] IDurableOrchestrationClient jobClient,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var connectedDevices = userDto.Value.ConnectedDevices ?? Array.Empty<Identifier>();
                var disconnectedDevices = userDto.Value.DisconnectedDevices ?? Array.Empty<Identifier>();
                var mobileDevice = userDto.Value.MobileDevice;

                var (userId, givenName, familyName) = await auth.ValidateUser(authorization, cancellationToken);
                var consent = await consentStore.FetchConsent(userId, cancellationToken);
                consent ??= ConsentFactory.WithId(userId);

                var statusCode = HttpStatusCode.NoContent;
                var jobId = (string?)null;

                if (consent.FhirId == null)
                {
                    var patient = await fhirClient.CreatePatient(userId, familyName, givenName, cancellationToken);
                    consent.FhirId = patient.Id;
                    statusCode = HttpStatusCode.OK;
                }

                if (mobileDevice != null)
                {
                    consent.MobileDevices.Add(mobileDevice);
                    statusCode = HttpStatusCode.OK;
                }

                if (connectedDevices.Length > 0 || disconnectedDevices.Length > 0)
                {
                    consent.Devices.RemoveAll(disconnectedDevices.Contains);
                    consent.Devices.AddRange(connectedDevices);
                    statusCode = HttpStatusCode.Accepted;
                }

                if (statusCode != HttpStatusCode.NoContent)
                {
                    await consentStore.WriteConsent(consent, cancellationToken);
                }

                if (connectedDevices.Length > 0 || disconnectedDevices.Length > 0)
                {
                    jobId = await jobClient.RunSingleton(
                        userId,
                        workflow: nameof(DeviceImportJob.DeviceImportWorkflow),
                        jobArguments: new StartDeviceManagementMessage
                        {
                            UserId = userId,
                            ConnectedDevices = connectedDevices,
                            DisconnectedDevices = disconnectedDevices,
                        },
                        log,
                        guidFactory);
                }

                return new JsonResult(await userFactory.CreateUser(consent, jobId, cancellationToken))
                {
                    StatusCode = (int?)statusCode,
                };
            });
        }
    }
}
