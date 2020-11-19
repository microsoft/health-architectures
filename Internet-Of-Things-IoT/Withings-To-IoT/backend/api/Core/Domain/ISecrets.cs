// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISecrets
    {
        Task<string?> FetchSecret(string key, CancellationToken cancellationToken);

        Task CreateSecret(string key, string value, DateTimeOffset expiry, CancellationToken cancellationToken);

        Task DeleteSecret(string key, CancellationToken cancellationToken);
    }
}
