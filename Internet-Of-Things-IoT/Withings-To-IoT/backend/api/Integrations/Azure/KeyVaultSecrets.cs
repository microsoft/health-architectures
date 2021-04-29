// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure;
    using global::Azure.Security.KeyVault.Secrets;
    using H3.Core.Domain;

    /// <summary>
    /// This class implements the H3 secret store pattern using Azure KeyVault secret storage.
    /// <a href="https://docs.microsoft.com/en-us/azure/key-vault/secrets/about-secrets">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// The implementation of this class assumes that the underlying KeyVault resource is
    /// soft-delete enabled as the ability to opt-out of soft-delete will be removed at
    /// the end of 2020.
    /// <a href="https://docs.microsoft.com/en-us/azure/key-vault/general/soft-delete-change">See documentation.</a>
    /// </remarks>
    public class KeyVaultSecrets : ISecrets
    {
        private static readonly TimeSpan PurgeRetryInterval = TimeSpan.FromSeconds(30);
        private static readonly int MaxPurgeRetries = 10;

        private readonly SecretClient client;

        public KeyVaultSecrets(SecretClient client)
        {
            this.client = client;
        }

        public async Task CreateSecret(string key, string value, DateTimeOffset expiry, CancellationToken cancellationToken)
        {
            var existingSecret = await FetchRawSecret(key, cancellationToken);

            if (existingSecret?.Properties.ExpiresOn >= expiry)
            {
                return;
            }

            var secret = new KeyVaultSecret(key, value);

            secret.Properties.ExpiresOn = expiry;

            await client.SetSecretAsync(secret, cancellationToken);
        }

        public async Task DeleteSecret(string key, CancellationToken cancellationToken)
        {
            try
            {
                await client.StartDeleteSecretAsync(key, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                switch (ex.Status)
                {
                    case 404:
                    case 409:
                        break;

                    default:
                        throw;
                }
            }

            var purgeDone = false;

            for (var i = 0; i < MaxPurgeRetries && !purgeDone; i++)
            {
                try
                {
                    await client.PurgeDeletedSecretAsync(key, cancellationToken);
                    purgeDone = true;
                }
                catch (RequestFailedException ex)
                {
                    switch (ex.Status)
                    {
                        case 404:
                            purgeDone = true;
                            break;

                        case 409:
                            await Task.Delay(PurgeRetryInterval);
                            break;

                        default:
                            throw;
                    }
                }
            }
        }

        public async Task<string?> FetchSecret(string key, CancellationToken cancellationToken)
        {
            var secret = await FetchRawSecret(key, cancellationToken);

            return secret?.Value;
        }

        private async Task<KeyVaultSecret?> FetchRawSecret(string key, CancellationToken cancellationToken)
        {
            Response<KeyVaultSecret> response;

            try
            {
                response = await client.GetSecretAsync(key, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                switch (ex.Status)
                {
                    case 404:
                        return null;

                    default:
                        throw;
                }
            }

            var secret = response.Value;

            if (DateTime.UtcNow > secret.Properties.ExpiresOn)
            {
                return null;
            }

            return secret;
        }
    }
}
