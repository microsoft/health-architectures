// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Apis
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using NSwag.Annotations;

    /// <summary>
    /// This class implements an endpoint to check the health of the system and its dependencies.
    /// </summary>
    /// <remarks>
    /// This endpoint is explicitly not annotated with <see cref="Domain.PublicApiAttribute"/> since
    /// it is only expected to be called from within the Azure Functions internal network.
    /// <a href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/autoscale-get-started#health-check-path">See documentation.</a>
    /// </remarks>
    public class HealthEndpoint
    {
        [SwaggerResponse(200, typeof(string), Description = "OK")]
        [FunctionName(nameof(GetHealth))]
        public JsonResult GetHealth(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
        {
            // TODO: check if dependencies are healthy too
            return new JsonResult("OK");
        }
    }
}
