// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Apis
{
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// This class implements an endpoint which returns the Swagger API specification for the system.
    /// </summary>
    [PublicApi]
    public class SwaggerEndpoint
    {
        private readonly ISwaggerGenerator swaggerGenerator;

        public SwaggerEndpoint(ISwaggerGenerator swaggerGenerator)
        {
            this.swaggerGenerator = swaggerGenerator;
        }

        [FunctionName(nameof(GetSwagger))]
        public async Task<IActionResult> GetSwagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger.json")] HttpRequest req)
        {
            var swagger = await swaggerGenerator.GenerateSwagger();

            return new OkObjectResult(swagger);
        }
    }
}
