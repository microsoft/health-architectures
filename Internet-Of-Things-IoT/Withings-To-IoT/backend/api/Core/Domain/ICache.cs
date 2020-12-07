// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;
    using System.Threading.Tasks;

    public interface ICache
    {
        Task<T?> FetchItem<T>(string key)
            where T : class;

        Task DeleteItem(string key);

        Task CreateItem<T>(string key, T item, TimeSpan ttl)
            where T : class;
    }
}
