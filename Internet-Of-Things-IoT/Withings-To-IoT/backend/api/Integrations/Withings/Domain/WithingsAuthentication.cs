// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Integrations.Withings.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements authentication with the Withings API via the OAuth 2.0 protocol.
    /// <a href="http://developer.withings.com/oauth2/#section/OAuth/OAuth-2.0">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// This class assumes that the initial part of the OAuth 2.0 flow to acquire an authentication
    /// code has been performed by a client before calling into this class.
    /// </remarks>
    public class WithingsAuthentication : IWithingsAuthentication
    {
        private static readonly TimeSpan AccessTokenExpiryThreshold = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan RefreshTokenExpiryThreshold = TimeSpan.FromDays(5);
        private static readonly TimeSpan RefreshTokenValidity = TimeSpan.FromDays(365);

        private readonly ILogger log;
        private readonly IHttp http;
        private readonly ISecrets secrets;
        private readonly string clientId;
        private readonly string clientSecret;

        public WithingsAuthentication(ILoggerFactory log, IHttp http, ISecrets secrets, ISettings settings)
        {
            this.log = log.CreateLogger<WithingsAuthentication>();
            this.http = http;
            this.secrets = secrets;
            this.clientId = settings.GetSetting("WITHINGS_CLIENT_ID");
            this.clientSecret = settings.GetSetting("WITHINGS_CLIENT_SECRET");
        }

        public async Task<Tokens> FetchTokens(string userId, string code, string redirectUri, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching token for {userId} from Withings API with client {clientId} and redirect {redirectUri}", userId, clientId, redirectUri);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://wbsapi.withings.net/v2/oauth2"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "action", "requesttoken" },
                    { "grant_type", "authorization_code" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "code", code },
                    { "redirect_uri", redirectUri },
                }),
            };

            var tokens = await MakeRequest<Tokens>(userId, request, cancellationToken);

            await StoreTokens(userId, tokens, cancellationToken);

            return tokens;
        }

        public async Task<Tokens> FetchTokens(string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching Withings API tokens for {userId}", userId);

            var accessTokenTask = secrets.FetchSecret(AccessTokenKeyFor(userId), cancellationToken);
            var refreshTokenTask = secrets.FetchSecret(RefreshTokenKeyFor(userId), cancellationToken);

            var accessToken = await accessTokenTask;
            var refreshToken = await refreshTokenTask;

            if (refreshToken == null)
            {
                throw new UnknownWithingsUserException
                {
                    UserId = userId,
                };
            }

            if (accessToken == null)
            {
                return await RefreshTokens(userId, refreshToken, cancellationToken);
            }

            return new Tokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task DeleteTokens(string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Deleting tokens for {userId}", userId);

            await Task.WhenAll(
                secrets.DeleteSecret(AccessTokenKeyFor(userId), cancellationToken),
                secrets.DeleteSecret(RefreshTokenKeyFor(userId), cancellationToken));
        }

        public async Task<Tokens> RefreshTokens(string userId, string refreshToken, CancellationToken cancellationToken)
        {
            log.LogInformation("Refreshing tokens for {userId} from Withings API with client {clientId}", userId, clientId);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://wbsapi.withings.net/v2/oauth2"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "action", "requesttoken" },
                    { "grant_type", "refresh_token" },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "refresh_token", refreshToken },
                }),
            };

            var tokens = await MakeRequest<Tokens>(userId, request, cancellationToken);

            await StoreTokens(userId, tokens, cancellationToken);

            return tokens;
        }

        private static string AccessTokenKeyFor(string userId)
        {
            return $"{userId}-accesstoken";
        }

        private static string RefreshTokenKeyFor(string userId)
        {
            return $"{userId}-refreshtoken";
        }

        private async Task<T> MakeRequest<T>(string userId, HttpRequestMessage request, CancellationToken cancellationToken)
            where T : class
        {
            log.LogInformation("Making request to {uri} for user {userId}", request.RequestUri, userId);
            var response = await http.Send<Response<T>, WithingsApiException>(request, cancellationToken);
            log.LogInformation("Got response from {uri} for user {userId} with status {status}", request.RequestUri, userId, response.Status);

            return response.Status switch
            {
                0 => response.Body,

                _ => throw new WithingsApiException
                {
                    Status = response.Status,
                    Error = response.Error,
                },
            };
        }

        private async Task StoreTokens(string userId, Tokens tokens, CancellationToken cancellationToken)
        {
            log.LogInformation("Storing Withings API tokens for {userId}", userId);

            var accessTokenExpiry = DateTimeOffset.UtcNow
                .AddSeconds(tokens.ExpiresIn)
                .Subtract(AccessTokenExpiryThreshold);

            var refreshTokenExpiry = DateTimeOffset.UtcNow
                .Add(RefreshTokenValidity)
                .Subtract(RefreshTokenExpiryThreshold);

            await Task.WhenAll(
                secrets.CreateSecret(AccessTokenKeyFor(userId), tokens.AccessToken, accessTokenExpiry, cancellationToken),
                secrets.CreateSecret(RefreshTokenKeyFor(userId), tokens.RefreshToken, refreshTokenExpiry, cancellationToken));
        }
    }
}
