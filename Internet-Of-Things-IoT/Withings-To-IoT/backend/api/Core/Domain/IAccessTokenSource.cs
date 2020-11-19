// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAccessTokenSource
    {
        Task<string> FetchToken(string resource, CancellationToken cancellationToken);
    }
}
