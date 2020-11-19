// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Api;

    public interface IConsentStore
    {
        Task DeleteConsent(string userId, CancellationToken cancellationToken);

        Task<Consent?> FetchConsent(string userId, CancellationToken cancellationToken);

        Task<Consent?> FetchConsentByExternalId(string system, string value, CancellationToken cancellationToken);

        Task WriteConsent(Consent consent, CancellationToken cancellationToken);
    }
}
