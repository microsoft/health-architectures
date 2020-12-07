// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using StackExchange.Redis;

    /// <summary>
    /// This class implements the H3 cache pattern using Azure Cache for Redis.
    /// <a href="https://docs.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview">See documentation.</a>
    /// </summary>
    public class RedisCache : ICache
    {
        private readonly ConnectionMultiplexer client;
        private readonly IJson json;

        public RedisCache(
            ConnectionMultiplexer client,
            IJson json)
        {
            this.client = client;
            this.json = json;
        }

        public Task CreateItem<T>(string key, T item, TimeSpan ttl)
            where T : class
        {
            var cache = client.GetDatabase();
            return cache.StringSetAsync(key, json.Dump(item), ttl);
        }

        public Task DeleteItem(string key)
        {
            var cache = client.GetDatabase();
            return cache.KeyDeleteAsync(key);
        }

        public async Task<T?> FetchItem<T>(string key)
            where T : class
        {
            var cache = client.GetDatabase();

            var cacheResponse = await cache.StringGetAsync(key);

            if (cacheResponse.IsNullOrEmpty)
            {
                return null;
            }

            return json.Load<T>(cacheResponse);
        }
    }
}
