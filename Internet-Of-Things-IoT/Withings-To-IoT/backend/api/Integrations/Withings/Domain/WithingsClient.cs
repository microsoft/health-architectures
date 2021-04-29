// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Integrations.Withings.Models;
    using H3.Integrations.Withings.Runtime;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class interfaces with the Withings data API, specifically the following endpoints:
    /// <list type="bullet">
    ///   <item><description><a href="http://developer.withings.com/oauth2/#operation/measure-getmeas">Measurements</a></description></item>
    ///   <item><description><a href="http://developer.withings.com/oauth2/#operation/userv2-getdevice">Devices</a></description></item>
    /// </list>
    /// </summary>
    public class WithingsClient : IWithingsClient
    {
        private const int MaxRequestRetries = 2;
        private const int TokenExpiredStatus = 401;
        private const int BadSubscriptionRequestStatus = 293;
        private const int SubscriptionNotFoundStatus = 380;
        private const int ResponseSuccessStatus = 0;
        private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(5);

        private readonly ILogger log;
        private readonly IHttp http;
        private readonly IWithingsAuthentication authentication;
        private readonly ICache cache;
        private readonly Uri notificationCallbackEndpoint;

        public WithingsClient(ILoggerFactory log, IHttp http, ISettings settings, ICache cache, IWithingsAuthentication authentication)
        {
            this.log = log.CreateLogger<WithingsClient>();
            this.http = http;
            this.authentication = authentication;
            this.cache = cache;
            this.notificationCallbackEndpoint = new Uri($"{settings.GetSetting("API_PREFIX")}/{Apis.CallbackUrl}");
        }

        public async Task<Device[]> FetchDevices(string userId, CancellationToken cancellationToken)
        {
            var cacheKey = DevicesCacheKeyFor(userId);

            log.LogInformation("Fetching devices in cache for {userId}", userId);
            var cachedDevices = await cache.FetchItem<Device[]>(cacheKey);

            if (cachedDevices != null)
            {
                log.LogInformation("Found {count} devices in cache for {userId}", cachedDevices.Length, userId);
                return cachedDevices;
            }

            var tokens = await authentication.FetchTokens(userId, cancellationToken);

            var devices = await FetchDevices(userId, tokens.AccessToken, tokens.RefreshToken, cancellationToken);

            log.LogInformation("Storing {count} devices in cache for {userId}", devices.Items.Length, userId);
            await cache.CreateItem(cacheKey, devices.Items, CacheTTL);

            return devices.Items;
        }

        public async Task<Group[]> FetchMeasurements(string userId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken)
        {
            var tokens = await authentication.FetchTokens(userId, cancellationToken);

            return await FetchMeasurements(userId, startDate, endDate, tokens.AccessToken, tokens.RefreshToken, cancellationToken);
        }

        public async Task DeleteAccount(string userId, CancellationToken cancellationToken)
        {
            log.LogInformation("Deleting account for {userId}", userId);

            var tokens = await authentication.FetchTokens(userId, cancellationToken);

            await DeleteSubscriptions(userId, tokens.AccessToken, tokens.RefreshToken, cancellationToken);

            await Task.WhenAll(
                cache.DeleteItem(DevicesCacheKeyFor(userId)),
                authentication.DeleteTokens(userId, cancellationToken));
        }

        public async Task<string> CreateAccount(string userId, string withingsAccessCode, string withingsRedirectUri, CancellationToken cancellationToken)
        {
            var tokens = await authentication.FetchTokens(userId, withingsAccessCode, withingsRedirectUri, cancellationToken);

            await SetupSubscriptions(userId, tokens.AccessToken, tokens.RefreshToken, cancellationToken);

            return tokens.UserId;
        }

        private static string DevicesCacheKeyFor(string userId)
        {
            return $"{userId}-devices";
        }

        private static long ToUnixTime(DateTimeOffset date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        private async Task DeleteSubscriptions(string userId, string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            log.LogInformation("Deleting Withings subscription for {userId}", userId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://wbsapi.withings.net/notify"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "action", "revoke" },
                    { "callbackurl", notificationCallbackEndpoint.ToString() },
                }),
            };

            try
            {
                await MakeRequestWithRetries<object>(userId, request, accessToken, refreshToken, cancellationToken);
            }
            catch (WithingsApiException ex) when (ex.Status == BadSubscriptionRequestStatus)
            {
                log.LogWarning("Skipping deletion of Withings subscription for user {userId} for this environment: {error}", userId, ex.Error);
            }
            catch (WithingsApiException ex) when (ex.Status == SubscriptionNotFoundStatus)
            {
                log.LogWarning("Attempted to delete Withings subscription for user {userId} before it was established: {error}", userId, ex.Error);
            }
        }

        private async Task SetupSubscriptions(string userId, string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            log.LogInformation("Setting up Withings subscription for {userId}", userId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://wbsapi.withings.net/notify"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "action", "subscribe" },
                    { "callbackurl", notificationCallbackEndpoint.ToString() },
                    { "comment", userId },
                }),
            };

            try
            {
                await MakeRequestWithRetries<object>(userId, request, accessToken, refreshToken, cancellationToken);
            }
            catch (WithingsApiException ex) when (ex.Status == BadSubscriptionRequestStatus)
            {
                log.LogWarning("Skipping setting up Withings subscription for user {userId} for this environment: {error}", userId, ex.Error);
            }
        }

        private Task<Devices> FetchDevices(string userId, string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching Withings devices for {userId}", userId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://wbsapi.withings.net/v2/user"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "action", "getdevice" },
                }),
            };

            return MakeRequestWithRetries<Devices>(userId, request, accessToken, refreshToken, cancellationToken);
        }

        private async Task<Group[]> FetchMeasurements(string userId, DateTimeOffset? startDate, DateTimeOffset? endDate, string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            var result = new List<Group>();
            int? offset = null;
            bool hasMore;

            do
            {
                var measures = await FetchMeasurements(userId, offset, startDate, endDate, accessToken, refreshToken, cancellationToken);

                result.AddRange(measures.MeasureGroups);
                hasMore = measures.HasMore == 1;
                offset = measures.Offset;
            }
            while (hasMore);

            return result.ToArray();
        }

        private Task<Measures> FetchMeasurements(string userId, int? offset, DateTimeOffset? startDate, DateTimeOffset? endDate, string accessToken, string refreshToken, CancellationToken cancellationToken)
        {
            log.LogInformation("Fetching Withings measurements for {userId}: offset={offset} startDate={startDate} endDate={endDate}", userId, offset, startDate, endDate);

            var measuresRequestArgs = new Dictionary<string, string>
            {
                { "action", "getmeas" },
            };

            if (offset != null)
            {
                measuresRequestArgs["offset"] = offset.Value.ToString();
            }

            if (startDate != null)
            {
                measuresRequestArgs["startdate"] = ToUnixTime(startDate.Value).ToString();
            }

            if (endDate != null)
            {
                measuresRequestArgs["enddate"] = ToUnixTime(endDate.Value).ToString();
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://wbsapi.withings.net/measure"),
                Content = new FormUrlEncodedContent(measuresRequestArgs),
            };

            return MakeRequestWithRetries<Measures>(userId, request, accessToken, refreshToken, cancellationToken);
        }

        private async Task<T> MakeRequestWithRetries<T>(string userId, HttpRequestMessage request, string accessToken, string refreshToken, CancellationToken cancellationToken)
            where T : class
        {
            var response = (Response<T>?)null;

            for (var i = 0; i < MaxRequestRetries; i++)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                log.LogInformation("Making request to {uri} for user {userId}", request.RequestUri, userId);
                response = await http.Send<Response<T>, WithingsApiException>(request, cancellationToken);
                log.LogInformation("Got response from {uri} for user {userId} with status {status}", request.RequestUri, userId, response.Status);

                switch (response.Status)
                {
                    case ResponseSuccessStatus:
                        return response.Body;

                    case TokenExpiredStatus:
                        var tokens = await authentication.RefreshTokens(userId, refreshToken, cancellationToken);
                        accessToken = tokens.AccessToken;
                        refreshToken = tokens.RefreshToken;
                        break;

                    default:
                        throw new WithingsApiException
                        {
                            Error = response.Error,
                            Status = response.Status,
                        };
                }
            }

            throw new WithingsApiException
            {
                Error = response!.Error,
                Status = response.Status,
            };
        }
    }
}
