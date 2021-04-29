// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Apis
{
    using System.Threading;
    using System.Threading.Tasks;
    using AzureFunctionsV2.HttpExtensions.Annotations;
    using AzureFunctionsV2.HttpExtensions.Infrastructure;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using NSwag.Annotations;

    /// <summary>
    /// This class implements an endpoint which lets clients track the status of long-running operations.
    /// </summary>
    [PublicApi]
    public class JobsEndpoint
    {
        private readonly ILogger<JobsEndpoint> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IAuthentication auth;

        public JobsEndpoint(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IAuthentication auth)
        {
            this.log = log.CreateLogger<JobsEndpoint>();
            this.exceptionFilter = exceptionFilter;
            this.auth = auth;
        }

        [SwaggerResponse(200, typeof(JobStatus), Description = "Job status")]
        [SwaggerResponse(401, typeof(string), Description = "Unauthorized")]
        [SwaggerResponse(503, typeof(string), Description = "Temporarily unavailable")]
        [FunctionName(nameof(GetJobStatus))]
        public Task<JsonResult> GetJobStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobs")] HttpRequest req,
            [HttpQuery(Required = true)]HttpParam<string> jobId,
            [HttpHeader(Name = "Authorization")]HttpParam<string> authorization,
            [DurableClient] IDurableOrchestrationClient orchestrationClient,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var (userId, _, _) = await auth.ValidateUser(authorization, cancellationToken);

                log.LogInformation("Fetching status of job {jobId} for user {userId}", jobId, userId);
                var status = await orchestrationClient.GetStatusAsync(jobId);
                log.LogInformation("Fetched status of job {jobId} for user {userId}", jobId, userId);

                return new JsonResult(new JobStatus
                {
                    Status = status.RuntimeStatus,
                });
            });
        }
    }
}
