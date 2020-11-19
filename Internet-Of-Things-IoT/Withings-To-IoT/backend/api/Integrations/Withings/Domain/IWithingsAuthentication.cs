// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Integrations.Withings.Models;

    /// <summary>
    /// This interface should not be referenced outside of the <see cref="Withings"/> namespace.
    /// </summary>
    public interface IWithingsAuthentication
    {
        Task DeleteTokens(string userId, CancellationToken cancellationToken);

        Task<Tokens> FetchTokens(string userId, CancellationToken cancellationToken);

        Task<Tokens> FetchTokens(string userId, string code, string redirectUri, CancellationToken cancellationToken);

        Task<Tokens> RefreshTokens(string userId, string refreshToken, CancellationToken cancellationToken);
    }
}
