// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureFunctionsV2.HttpExtensions.Annotations;
    using AzureFunctionsV2.HttpExtensions.Infrastructure;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Core.Models.Fhir;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    /// <summary>
    /// This class implements an endpoint which returns all IoMT data for a user.
    /// </summary>
    [PublicApi]
    public class ObservationsEndpoint
    {
        private readonly ILogger<ObservationsEndpoint> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IAuthentication auth;
        private readonly IConsentStore consentStore;
        private readonly IFhirClient fhirClient;

        public ObservationsEndpoint(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IAuthentication auth,
            IConsentStore consentStore,
            IFhirClient fhirClient)
        {
            this.log = log.CreateLogger<ObservationsEndpoint>();
            this.exceptionFilter = exceptionFilter;
            this.auth = auth;
            this.consentStore = consentStore;
            this.fhirClient = fhirClient;
        }

        [SwaggerResponse(200, typeof(List<Observation>), Description = "List of FHIR observations")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(404, typeof(string), Description = "Not Found")]
        [SwaggerResponse(409, typeof(string), Description = "Conflict")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(GetObservations))]
        public Task<JsonResult> GetObservations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "observations")] HttpRequest req,
            [HttpHeader(Name = "Authorization")]HttpParam<string> authorization,
            [HttpQuery]HttpParam<long?> afterEpoch,
            [HttpQuery]HttpParam<long?> beforeEpoch,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var (userId, _, _) = await auth.ValidateUser(authorization, cancellationToken);
                var consent = await consentStore.FetchConsent(userId, cancellationToken);

                var observations = await fhirClient.FetchObservations(
                    userId,
                    fhirUserId: consent.Verify(userId).FhirId,
                    after: ToDateTimeOffset(afterEpoch),
                    before: ToDateTimeOffset(beforeEpoch),
                    cancellationToken: cancellationToken);

                return new JsonResult(observations);
            });
        }

        private static DateTimeOffset? ToDateTimeOffset(long? epoch)
        {
            if (epoch == null)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeSeconds(epoch.Value);
        }
    }
}
