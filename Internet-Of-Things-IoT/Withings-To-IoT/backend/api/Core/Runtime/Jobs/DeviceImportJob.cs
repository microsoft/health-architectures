// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Runtime.Jobs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements a background job to ingest or delete data from IoMT vendors
    /// into the system.
    /// </summary>
    public class DeviceImportJob
    {
        private readonly ILogger<DeviceImportJob> log;
        private readonly IVendorClient vendorClient;

        public DeviceImportJob(
            ILoggerFactory log,
            IVendorClient vendorClient)
        {
            this.log = log.CreateLogger<DeviceImportJob>();
            this.vendorClient = vendorClient;
        }

        [FunctionName(nameof(DeviceImportWorkflow))]
        public Task DeviceImportWorkflow(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext)
        {
            var input = orchestrationContext.GetInput<StartDeviceManagementMessage>();
            var tasks = new List<Task>();

            tasks.AddRange(vendorClient.StartDeviceImportJobs(orchestrationContext, input.UserId, input.ConnectedDevices));
            tasks.AddRange(vendorClient.StartDeviceDeletionJobs(orchestrationContext, input.UserId, input.DisconnectedDevices));

            return Task.WhenAll(tasks);
        }
    }
}
