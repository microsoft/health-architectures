// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Fhir;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;

    public class MultiVendorClient : IVendorClient
    {
        private readonly IReadOnlyCollection<IVendorClient> clients;

        public MultiVendorClient(IReadOnlyCollection<IVendorClient> clients)
        {
            this.clients = clients;
        }

        public async Task<Ref[]> FetchDevices(string userId, CancellationToken cancellationToken)
        {
            var devices = await Task.WhenAll(clients.Select(client => client.FetchDevices(userId, cancellationToken)));

            return devices.SelectMany(clientDevices => clientDevices).ToArray();
        }

        public IEnumerable<Task> StartDeviceImportJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices)
        {
            return clients.SelectMany(client => client.StartDeviceImportJobs(context, userId, devices));
        }

        public IEnumerable<Task> StartDeviceDeletionJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices)
        {
            return clients.SelectMany(client => client.StartDeviceDeletionJobs(context, userId, devices));
        }

        public Task DeleteAccount(string userId, CancellationToken cancellationToken)
        {
            return Task.WhenAll(clients.Select(client => client.DeleteAccount(userId, cancellationToken)));
        }
    }
}
