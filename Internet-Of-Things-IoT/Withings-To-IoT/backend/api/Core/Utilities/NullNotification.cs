// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Extensions.Logging;

    public class NullNotification : INotification
    {
        private readonly ILogger<NullNotification> log;

        public NullNotification(ILoggerFactory log)
        {
            this.log = log.CreateLogger<NullNotification>();
        }

        public Task SendNotification(string userId, IReadOnlyCollection<MobileDevice> devices, string message, bool silent, CancellationToken cancellationToken)
        {
            log.LogInformation("Skipping sending of notifications for user {userId}", userId);

            return Task.CompletedTask;
        }
    }
}
