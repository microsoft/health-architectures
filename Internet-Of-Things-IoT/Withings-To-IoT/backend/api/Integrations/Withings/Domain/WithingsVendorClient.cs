// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Fhir;
    using H3.Integrations.Withings.Models;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;

    /// <summary>
    /// This class is the entrypoint into the Withings IoMT integration.
    /// </summary>
    public class WithingsVendorClient : IVendorClient
    {
        private readonly IWithingsClient client;
        private readonly IWithingsToFhirConverter converter;

        public WithingsVendorClient(IWithingsClient client, IWithingsToFhirConverter converter)
        {
            this.client = client;
            this.converter = converter;
        }

        public async Task<Ref[]> FetchDevices(string userId, CancellationToken cancellationToken)
        {
            var withingsDevices = await client.FetchDevices(userId, cancellationToken);

            return converter.Convert(withingsDevices, null).ToArray();
        }

        public IEnumerable<Task> StartDeviceImportJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices)
        {
            var withingsDevices = GetDevices(devices);

            if (withingsDevices.Length > 0)
            {
                yield return context.CallActivityAsync(nameof(Runtime.Jobs.WithingsRunDeviceIngestion), new StartDeviceIngestionMessage
                {
                    UserId = userId,
                    WithingsDeviceIds = withingsDevices.Select(device => device.Value).ToArray(),
                });
            }
        }

        public IEnumerable<Task> StartDeviceDeletionJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices)
        {
            var withingsDevices = GetDevices(devices);

            if (withingsDevices.Length > 0)
            {
                yield return context.CallActivityAsync(nameof(Runtime.Jobs.WithingsRunDeviceDeletion), new StartDeviceDeletionMessage
                {
                    UserId = userId,
                    WithingsDeviceIds = withingsDevices.Select(device => device.Value).ToArray(),
                });
            }
        }

        public Task DeleteAccount(string userId, CancellationToken cancellationToken)
        {
            return client.DeleteAccount(userId, cancellationToken);
        }

        private Identifier[] GetDevices(IReadOnlyCollection<Identifier> devices)
        {
            return devices.Where(device => device.System == converter.System).ToArray();
        }
    }
}
