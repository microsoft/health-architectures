// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Core.Models.Fhir;
    using Microsoft.Extensions.Logging;

    public class UserFactory : IUserFactory
    {
        private readonly ILogger<UserFactory> log;
        private readonly IVendorClient vendorClient;

        public UserFactory(ILoggerFactory log, IVendorClient vendorClient)
        {
            this.log = log.CreateLogger<UserFactory>();
            this.vendorClient = vendorClient;
        }

        public async Task<User> CreateUser(Consent consent, string? jobId, CancellationToken cancellationToken)
        {
            Ref[] devices;

            try
            {
                devices = await vendorClient.FetchDevices(consent.UserId, cancellationToken);
            }
            catch (H3Exception ex)
            {
                log.LogWarning("Ignoring exception {exception} when fetching devices for user {userId}", ex, consent.UserId);
                devices = Array.Empty<Ref>();
            }

            var connectedDevices = new List<Ref>();
            var disconnectedDevices = new List<Ref>();
            var consentedDevices = consent?.Devices;

            foreach (var device in devices)
            {
                if (consentedDevices != null && consentedDevices.Contains(device.Identifier))
                {
                    connectedDevices.Add(device);
                }
                else
                {
                    disconnectedDevices.Add(device);
                }
            }

            return new User
            {
                ConnectedDevices = connectedDevices.ToArray(),
                DisconnectedDevices = disconnectedDevices.ToArray(),
                JobId = jobId,
            };
        }
    }
}
