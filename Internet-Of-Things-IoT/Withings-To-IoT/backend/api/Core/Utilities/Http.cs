// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;

    public class Http : IHttp
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private readonly IHttpClientFactory factory;
        private readonly IJson json;

        public Http(
            IHttpClientFactory factory,
            IJson json)
        {
            this.factory = factory;
            this.json = json;
        }

        public async Task<TResult> Send<TResult, TException>(HttpRequestMessage request, CancellationToken cancellationToken)
            where TResult : class
            where TException : ApiException, new()
        {
            var client = factory.CreateClient();

            client.Timeout = Timeout;

            var response = await client.SendAsync(request, cancellationToken);

            await ThrowIfFailed<TException>(response);

            var responseContent = await response.Content.ReadAsStringAsync();

            return json.Load<TResult>(responseContent);
        }

        private static async Task ThrowIfFailed<TException>(HttpResponseMessage response)
            where TException : ApiException, new()
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                throw new TException
                {
                    Error = errorMessage,
                    Status = (int)response.StatusCode,
                };
            }
        }
    }
}
