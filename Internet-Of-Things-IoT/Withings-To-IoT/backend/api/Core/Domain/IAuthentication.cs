// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAuthentication
    {
        Task<(string userId, string? givenName, string? familyName)> ValidateUser(string headerValue, CancellationToken cancellationToken);
    }
}
