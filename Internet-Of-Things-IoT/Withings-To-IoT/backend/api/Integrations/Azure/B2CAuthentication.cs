// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// This class implements the H3 authentication pattern using Azure Active Directory B2C.
    /// <a href="https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview">See documentation.</a>
    /// </summary>
    public class B2CAuthentication : IAuthentication
    {
        private const int MaxAttempts = 2;

        private readonly ILogger log;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configManager;
        private readonly string issuer;
        private readonly string audience;

        public B2CAuthentication(ILoggerFactory log, IConfigurationManager<OpenIdConnectConfiguration> configManager, ISettings settings)
        {
            this.log = log.CreateLogger<B2CAuthentication>();
            this.configManager = configManager;
            this.issuer = settings.GetSetting("B2C_ISSUER");
            this.audience = settings.GetSetting("B2C_AUDIENCE");
        }

        public async Task<(string userId, string? givenName, string? familyName)> ValidateUser(string headerValue, CancellationToken cancellationToken)
        {
            if (!AuthenticationHeaderValue.TryParse(headerValue, out var header))
            {
                throw new B2CAuthenticationException
                {
                    Problem = "Failed to parse authentication header",
                };
            }

            if (header.Scheme != "Bearer")
            {
                throw new B2CAuthenticationException
                {
                    Problem = "Unsupported authentication scheme",
                };
            }

            var config = await configManager.GetConfigurationAsync(cancellationToken);

            var validationParameter = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = audience,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys,
            };

            var principal = (ClaimsPrincipal?)null;

            for (var i = 0; i < MaxAttempts && principal == null; i++)
            {
                try
                {
                    principal = new JwtSecurityTokenHandler().ValidateToken(header.Parameter, validationParameter, out var token);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    log.LogInformation("Refreshing OpenIDConnect configuration");
                    configManager.RequestRefresh();
                }
                catch (ArgumentException)
                {
                    throw new B2CAuthenticationException
                    {
                        Problem = "Unable to parse identity token",
                    };
                }
                catch (SecurityTokenException)
                {
                    throw new B2CAuthenticationException
                    {
                        Problem = "Invalid identity token",
                    };
                }
            }

            return (
                userId: GetUserId(principal),
                givenName: GetGivenName(principal),
                familyName: GetFamilyName(principal));
        }

        private static string GetUserId(ClaimsPrincipal? principal)
        {
            var userId = principal?.Claims?.FirstOrDefault(claim =>
                claim.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier" ||
                claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?.Value;

            if (userId == null)
            {
                throw new B2CAuthenticationException
                {
                    Problem = "No identifier found in token",
                };
            }

            return userId;
        }

        private static string? GetGivenName(ClaimsPrincipal? principal)
        {
            return principal?.Claims?.FirstOrDefault(claim =>
                claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
            ?.Value;
        }

        private static string? GetFamilyName(ClaimsPrincipal? principal)
        {
            return principal?.Claims?.FirstOrDefault(claim =>
                claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
            ?.Value;
        }
    }

    public class B2CAuthenticationException : AuthenticationError
    {
    }
}
