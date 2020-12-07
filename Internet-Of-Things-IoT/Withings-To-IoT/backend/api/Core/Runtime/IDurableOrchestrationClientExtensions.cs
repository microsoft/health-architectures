// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime
{
    using System;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class provides  methods for Azure Durable Functions orchestrations:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="RunSingleton"/> leverages the singleton orchestrator pattern to provide a
    ///     method that ensures only a single background job is running for a specific user.
    ///     This pattern implements a distributed lock and makes it easier to avoid issues
    ///     arising from multiple requests concurrently modifying a user's account by providing
    ///     a critical section. For scenarios in which concurrent processing is desired, consider leveraging <see cref="ServiceBusQueue"/>.
    ///     <a href="https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons">See documentation.</a>
    ///   </description></item>
    /// </list>
    /// <a href="https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations">See documentation.</a>
    /// </summary>
    public static class IDurableOrchestrationClientExtensions
    {
        private static readonly TimeSpan WaitForJobPollingIntervall = TimeSpan.FromSeconds(30);

        public static async Task<string> RunSingleton<T>(this IDurableOrchestrationClient client, string userId, string workflow, T jobArguments, ILogger log, IGuidFactory guidFactory, bool blocking = false)
            where T : class
        {
            while (true)
            {
                var jobIsRunning = await IsJobRunning(client, userId, log, guidFactory);

                if (!jobIsRunning)
                {
                    break;
                }

                if (!blocking)
                {
                    throw new BackgroundJobIsRunningException();
                }

                log.LogInformation("Workflow for user {userId} is still running", userId);
                await Task.Delay(WaitForJobPollingIntervall);
            }

            log.LogInformation("Starting workflow {workflow} for user {userId}", workflow, userId);
            return await client.StartNewAsync(workflow, ToInstanceId(userId, guidFactory), jobArguments);
        }

        private static async Task<bool> IsJobRunning(IDurableOrchestrationClient client, string userId, ILogger log, IGuidFactory guidFactory)
        {
            log.LogInformation("Checking if any workflow is running for user {userId}", userId);
            var existingJob = await client.GetStatusAsync(ToInstanceId(userId, guidFactory));

            return existingJob?.RuntimeStatus switch
            {
                null => false,
                OrchestrationRuntimeStatus.Unknown => false,
                OrchestrationRuntimeStatus.Completed => false,
                OrchestrationRuntimeStatus.Failed => false,
                OrchestrationRuntimeStatus.Canceled => false,
                OrchestrationRuntimeStatus.Terminated => false,
                OrchestrationRuntimeStatus.Running => true,
                OrchestrationRuntimeStatus.ContinuedAsNew => true,
                OrchestrationRuntimeStatus.Pending => true,
                _ => throw new InvalidOperationException(),
            };
        }

        private static string ToInstanceId(string userId, IGuidFactory guidFactory)
        {
            return guidFactory.Create(userId).ToString("N");
        }
    }
}
