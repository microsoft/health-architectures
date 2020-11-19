// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using Microsoft.Azure.Services.AppAuthentication;

    /// <summary>
    /// This class implements the H3 access token pattern using Azure Managed Identity.
    /// <a href="https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// For local development purposes, the implementation contains a fall-back which checks
    /// role assignments of the current logged-in user against Azure Active Directory, so using
    /// <c>az login</c> combined with <c>az role assignment create</c> will enable running this
    /// code outside of the context of Azure infrastructure.
    /// </remarks>
    public class ManagedIdentity : IAccessTokenSource
    {
        private readonly AzureServiceTokenProvider provider;

        public ManagedIdentity(AzureServiceTokenProvider provider)
        {
            this.provider = provider;
        }

        public Task<string> FetchToken(string resource, CancellationToken cancellationToken)
        {
            return provider.GetAccessTokenAsync(resource, cancellationToken: cancellationToken);
        }
    }
}
