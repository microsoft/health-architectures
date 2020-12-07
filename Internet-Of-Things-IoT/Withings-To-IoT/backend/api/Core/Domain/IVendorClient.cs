// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Fhir;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;

    public interface IVendorClient
    {
        Task<Ref[]> FetchDevices(string userId, CancellationToken cancellationToken);

        IEnumerable<Task> StartDeviceImportJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices);

        IEnumerable<Task> StartDeviceDeletionJobs(IDurableOrchestrationContext context, string userId, IReadOnlyCollection<Identifier> devices);

        Task DeleteAccount(string userId, CancellationToken cancellationToken);
    }
}
