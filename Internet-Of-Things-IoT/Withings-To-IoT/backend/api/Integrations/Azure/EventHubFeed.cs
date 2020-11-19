// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.Messaging.EventHubs;
    using global::Azure.Messaging.EventHubs.Producer;
    using global::Newtonsoft.Json;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements the H3 event feed pattern using Azure Event Hubs and Databricks.
    /// Messages are sent to an Event Hub and automatically captured to an Azure Data Lake
    /// Storage gen2 account from where Databricks can process them.
    /// <a href="https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-capture-overview">See documentation.</a>
    /// </summary>
    public class EventHubFeed : IEventFeed
    {
        private readonly EventHubProducerClient client;
        private readonly ILogger log;
        private readonly IHttp http;
        private readonly IJson json;
        private readonly string endpoint;
        private readonly string token;
        private readonly long deleteUserJobId;

        public EventHubFeed(
            EventHubProducerClient client,
            ILoggerFactory log,
            IHttp http,
            IJson json,
            ISettings settings)
        {
            this.client = client;
            this.log = log.CreateLogger<EventHubFeed>();
            this.http = http;
            this.json = json;
            this.endpoint = settings.GetSetting("DATABRICKS_HOST");
            this.token = settings.GetSetting("DATABRICKS_TOKEN");
            this.deleteUserJobId = long.Parse(settings.GetSetting("DATABRICKS_DELETE_USER_JOB_ID"));
        }

        public async Task SendMessages<T>(IEnumerable<T> messages, CancellationToken cancellationToken)
            where T : class
        {
            var batch = await client.CreateBatchAsync();

            foreach (var message in messages)
            {
                var outputMessage = json.Dump<T>(message);

                if (!batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(outputMessage))))
                {
                    throw new InvalidOperationException($"Unable to add message {outputMessage} to batch");
                }
            }

            await client.SendAsync(batch, cancellationToken);
        }

        public async Task DeleteUser(string fhirUserId, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{endpoint}/api/2.0/jobs/run-now"),
                Method = HttpMethod.Post,
                Content = new StringContent(json.Dump(new DatabricksRunNowRequest
                {
                    JobId = deleteUserJobId,
                    Parameters = new Dictionary<string, string>
                    {
                        { "fhirUserId", $"Patient/{fhirUserId}" },
                    },
                })),
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            log.LogInformation("Making request to {endpoint} for user {fhirUserId}", request.RequestUri, fhirUserId);
            var databricksRun = await http.Send<DatabricksRunNowResponse, DatabricksApiException>(request, cancellationToken);
            log.LogInformation("Started run {runId} for user {fhirUserId}", databricksRun.RunId, fhirUserId);
        }
    }

    public class DatabricksApiException : ApiException
    {
    }

    /// <summary>
    /// Request to instruct Databricks to run a job.
    /// <a href="https://docs.databricks.com/dev-tools/api/latest/jobs.html#run-now">See documentation.</a>
    /// </summary>
    public class DatabricksRunNowRequest
    {
        [JsonProperty("job_id")]
        public long JobId { get; set; }

        [JsonProperty("notebook_params")]
        public Dictionary<string, string> Parameters { get; set; } = null!;
    }

    /// <summary>
    /// Response from Databricks when requested to run a job.
    /// <a href="https://docs.databricks.com/dev-tools/api/latest/jobs.html#run-now">See documentation.</a>
    /// </summary>
    public class DatabricksRunNowResponse
    {
        [JsonProperty("run_id")]
        public long RunId { get; set; }
    }
}
