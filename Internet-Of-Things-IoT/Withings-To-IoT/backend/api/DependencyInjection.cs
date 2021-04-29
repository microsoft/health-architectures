// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3
{
    using System;
    using Azure.Identity;
    using Azure.Messaging.EventHubs.Producer;
    using Azure.Security.KeyVault.Secrets;
    using H3.Core.Domain;
    using H3.Core.Utilities;
    using H3.Integrations.Azure;
    using H3.Integrations.Newtonsoft;
    using H3.Integrations.Withings.Domain;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.NotificationHubs;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using StackExchange.Redis;

    /// <summary>
    /// This class is responsible for registering services for dependency injection.
    /// <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection">See documentation.</a>
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureH3(this IServiceCollection services)
        {
            services.AddSingleton<IAuthentication, B2CAuthentication>();
            services.AddSingleton<IFhirClient, FhirApiClient>();
            services.AddSingleton<ISwaggerGenerator, FunctionsSwaggerGenerator>();
            services.AddSingleton<IUserFactory, UserFactory>();

            services.AddSingleton<IWithingsClient, WithingsClient>();
            services.AddSingleton<IWithingsAuthentication, WithingsAuthentication>();
            services.AddSingleton<IWithingsToFhirConverter, WithingsToFhirConverter>();

            services.AddSingleton<IVendorClient>((s) =>
            {
                return new MultiVendorClient(new IVendorClient[]
                {
                    new WithingsVendorClient(
                        client: s.GetRequiredService<IWithingsClient>(),
                        converter: s.GetRequiredService<IWithingsToFhirConverter>()),
                });
            });

            services.AddSingleton<IExceptionFilter>((s) =>
            {
                var log = s.GetRequiredService<ILoggerFactory>();

                return new ExceptionFilter(new IErrorHandler[]
                {
                    new BaseErrorHandler(log),
                    new WithingsErrorHandler(log),
                });
            });

            var settings = new EnvironmentSettings();

            services.AddSingleton<ISettings>(settings);

            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>((s) =>
            {
                var policyName = settings.GetSetting("B2C_POLICY_NAME");
                var issuer = settings.GetSetting("B2C_ISSUER");

                return new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{issuer}/.well-known/openid-configuration?p={policyName}",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());
            });

            services.AddSingleton<INotification>((s) =>
            {
                if (settings.GetSetting("NH_DISABLE") == "1")
                {
                    return new NullNotification(
                        log: s.GetRequiredService<ILoggerFactory>());
                }

                return new NotificationHub(
                    log: s.GetRequiredService<ILoggerFactory>(),
                    json: s.GetRequiredService<IJson>(),
                    client: new NotificationHubClient(
                        connectionString: settings.GetSetting("NH_CONNECTION_STRING_FULL"),
                        notificationHubPath: settings.GetSetting("NH_NAME")));
            });

            services.AddSingleton<IQueue>((s) =>
            {
                return new ServiceBusQueue(
                    connectionString: settings.GetSetting("SB_CONNECTION_STRING"),
                    json: s.GetRequiredService<IJson>());
            });

            services.AddSingleton<IConsentStore>((s) =>
            {
                var log = s.GetRequiredService<ILoggerFactory>();

                var account = new CosmosClient(settings.GetSetting("COSMOS_CONNECTION_STRING"));
                var database = account.GetDatabase(settings.GetSetting("COSMOS_DATABASE_NAME"));
                var container = database.GetContainer(settings.GetSetting("COSMOS_CONTAINER_NAME"));

                return new CosmosConsentStore(log, container);
            });

            services.AddSingleton<IGuidFactory>((s) =>
            {
                return new GuidFactory();
            });

            services.AddSingleton<IAccessTokenSource>((s) =>
            {
                return new ManagedIdentity(new AzureServiceTokenProvider());
            });

            services.AddSingleton<ICache>((s) =>
            {
                var hostName = settings.GetSetting("REDIS_HOSTNAME");
                var password = settings.GetSetting("REDIS_PASSWORD");

                var connectionString = $"{hostName},password={password},abortConnect=false,ssl=true";

                return new RedisCache(
                    client: ConnectionMultiplexer.Connect(connectionString),
                    json: s.GetRequiredService<IJson>());
            });

            services.AddSingleton<ISecrets>((s) =>
            {
                return new KeyVaultSecrets(new SecretClient(
                    vaultUri: new Uri(settings.GetSetting("TOKEN_STORE_URL")),
                    credential: new DefaultAzureCredential()));
            });

            services.AddHttpClient();
            services.AddSingleton<IJson, Json>();
            services.AddSingleton<IHttp, Http>();

            return services;
        }
    }
}
