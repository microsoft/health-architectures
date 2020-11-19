// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Net;
    using H3.Core.Domain;
    using H3.Integrations.Withings.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class translates errors from the Withings data model to the H3 data model.
    /// </summary>
    public class WithingsErrorHandler : IErrorHandler
    {
        private static readonly TimeSpan RateLimit = TimeSpan.FromMinutes(1);

        private readonly ILogger<WithingsErrorHandler> log;

        public WithingsErrorHandler(ILoggerFactory log)
        {
            this.log = log.CreateLogger<WithingsErrorHandler>();
        }

        public JsonResult? CanHandleResponse(Exception exception)
        {
            if (exception is UnknownWithingsUserException)
            {
                return new JsonResult("Must authenticate with Withings")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized,
                };
            }
            else if (exception is WithingsApiException withingsApiException)
            {
                if (withingsApiException.Status == 601)
                {
                    return new JsonResult("The system is busy")
                    {
                        StatusCode = (int?)HttpStatusCode.TooManyRequests,
                    };
                }
                else if (withingsApiException.Status == 293 && withingsApiException.Error.Contains("wrong redirect_uri", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new JsonResult("Invalid Withings redirect URI")
                    {
                        StatusCode = (int?)HttpStatusCode.Unauthorized,
                    };
                }
                else if (withingsApiException.Status == 503 && withingsApiException.Error.Contains("invalid refresh_token", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new JsonResult("Must re-authenticate with Withings")
                    {
                        StatusCode = (int?)HttpStatusCode.Unauthorized,
                    };
                }
                else if (withingsApiException.Status == 503 && withingsApiException.Error.Contains("invalid code", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new JsonResult("Invalid Withings access code")
                    {
                        StatusCode = (int?)HttpStatusCode.Unauthorized,
                    };
                }
            }

            return null;
        }

        public (bool canHandle, TimeSpan? retryAfter) CanHandle(Exception exception)
        {
            if (exception is WithingsApiException withingsApiException)
            {
                if (withingsApiException.Status == 601)
                {
                    log.LogInformation("Waiting for Withings rate limit to reset");
                    return (canHandle: true, retryAfter: RateLimit);
                }
            }
            else if (exception is UnknownWithingsUserException unknownWithingsUserException)
            {
                log.LogWarning("Skipping job as there are no Withings credentials for {userId}", unknownWithingsUserException.UserId);
                return (canHandle: true, retryAfter: null);
            }

            return (canHandle: false, retryAfter: null);
        }
    }
}
