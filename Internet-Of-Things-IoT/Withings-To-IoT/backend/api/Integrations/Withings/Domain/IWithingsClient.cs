// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Integrations.Withings.Models;

    /// <summary>
    /// This interface should not be referenced outside of the <see cref="Withings"/> namespace.
    /// </summary>
    public interface IWithingsClient
    {
        Task<string> CreateAccount(string userId, string withingsAccessCode, string withingsRedirectUri, CancellationToken cancellationToken);

        Task DeleteAccount(string userId, CancellationToken cancellationToken);

        Task<Device[]> FetchDevices(string userId, CancellationToken cancellationToken);

        Task<Group[]> FetchMeasurements(string userId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken);
    }
}
