// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Api;

    public interface IUserFactory
    {
        Task<User> CreateUser(Consent consent, string? jobId, CancellationToken cancellationToken);
    }
}
