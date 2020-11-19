// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(H3.Startup))]

namespace H3
{
    /// <summary>
    /// This class is responsible for configuring the Azure Functions hosting environment,
    /// including registering services for dependency injection.
    /// <a href="https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection">See documentation.</a>
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureH3();
        }
    }
}
