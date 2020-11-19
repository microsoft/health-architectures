// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Api;

    public interface IHttp
    {
        Task<TResult> Send<TResult, TException>(HttpRequestMessage request, CancellationToken cancellationToken)
            where TResult : class
            where TException : ApiException, new();
    }
}
