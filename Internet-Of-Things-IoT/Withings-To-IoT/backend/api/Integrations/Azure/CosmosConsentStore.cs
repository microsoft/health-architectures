// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements the H3 consent store using Azure Cosmos DB's SQL API.
    /// <a href="https://docs.microsoft.com/en-us/azure/cosmos-db/introduction">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// The implementation of this class assumes that all the members of <see cref="Consent.ExternalIds"/>
    /// are indexed and that <see cref="Consent.UserId"/> is set up as the collection's partition key.
    /// </remarks>
    public class CosmosConsentStore : IConsentStore
    {
        private readonly ILogger log;
        private readonly Container container;

        public CosmosConsentStore(ILoggerFactory log, Container container)
        {
            this.log = log.CreateLogger<CosmosConsentStore>();
            this.container = container;
        }

        public async Task<Consent?> FetchConsent(string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching record {userId}", userId);
            var (consent, charge) = await ReadItem<Consent>(userId, cancellationToken);
            log.LogInformation("{found} record for {userId}, costing {charge}", consent != null ? "Fetched" : "Missing", userId, charge);

            return consent;
        }

        public async Task<Consent?> FetchConsentByExternalId(string system, string value, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching records {system}={value}", system, value);
            var (consent, charge) = await ReadItems<Consent>($"externalIds.{system}", value, cancellationToken);
            log.LogInformation("Fetched {count} records for {system}={value}, costing {charge}", consent.Count, system, value, charge);

            return consent.Count != 1 ? null : consent.First();
        }

        public async Task WriteConsent(Consent consent, CancellationToken cancellationToken)
        {
            log.LogInformation("Writing record {userId}", consent.UserId);
            var response = await container.UpsertItemAsync(consent, new PartitionKey(consent.UserId), cancellationToken: cancellationToken);
            log.LogInformation("Wrote record {userId}, costing {charge}", consent.UserId, response.RequestCharge);
        }

        public async Task DeleteConsent(string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Deleting record {userId}", userId);

            try
            {
                var response = await container.DeleteItemAsync<Consent>(userId, new PartitionKey(userId), cancellationToken: cancellationToken);
                log.LogInformation("Deleted record {userId}, costing {charge}", userId, response.RequestCharge);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                log.LogInformation("Skipped deleting record {userId} as it's already gone, costing {charge}", userId, ex.RequestCharge);
            }
        }

        private async Task<(List<T> items, double charge)> ReadItems<T>(string column, object value, CancellationToken cancellationToken)
            where T : class
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.{column} = @value")
                .WithParameter("@value", value);

            var results = new List<T>();
            var charge = 0d;

            using var search = container.GetItemQueryIterator<T>(query);

            while (search.HasMoreResults)
            {
                var response = await search.ReadNextAsync(cancellationToken);

                results.AddRange(response.Resource);
                charge += response.RequestCharge;
            }

            return (results, charge);
        }

        private async Task<(T? item, double charge)> ReadItem<T>(string key, CancellationToken cancellationToken)
            where T : class
        {
            ItemResponse<T> response;

            try
            {
                response = await container.ReadItemAsync<T>(key, new PartitionKey(key), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                return (null, ex.RequestCharge);
            }

            return (response.Resource, response.RequestCharge);
        }
    }
}
