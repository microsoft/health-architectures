// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System.Text;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using Microsoft.Azure.ServiceBus;

    /// <summary>
    /// This class implements the H3 queue pattern using Azure Service Bus.
    /// <a href="https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// In general, the implicit queing functionality provided by <see cref="IDurableOrchestrationClientExtensions"/>
    /// should be preferred to explicit messaging via Service Bus as the former implements a
    /// distributed lock to simplify idempotency and race conditions. The Service Bus implementation
    /// is only provided for scenarios where the locking behavior isn't desirable such as for
    /// example if we can't afford to wait until a message can be sent to the implicit Durable
    /// Functions queue as is the case in the context of a webhook callback endpoint where the
    /// caller expects a response within a specific short timeframe.
    /// </remarks>
    public class ServiceBusQueue : IQueue
    {
        private readonly string connectionString;
        private readonly IJson json;

        public ServiceBusQueue(
            string connectionString,
            IJson json)
        {
            this.connectionString = connectionString;
            this.json = json;
        }

        public Task SendMessage<T>(string queueName, T message)
            where T : class
        {
            var client = new QueueClient(connectionString, queueName);

            return client.SendAsync(new Message(Encoding.UTF8.GetBytes(json.Dump(message))));
        }
    }
}
