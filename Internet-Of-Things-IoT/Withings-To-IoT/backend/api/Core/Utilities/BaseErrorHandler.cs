// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using AzureFunctionsV2.HttpExtensions.Exceptions;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class BaseErrorHandler : IErrorHandler
    {
        private readonly ILogger<BaseErrorHandler> log;

        public BaseErrorHandler(ILoggerFactory log)
        {
            this.log = log.CreateLogger<BaseErrorHandler>();
        }

        public JsonResult? CanHandleResponse(Exception exception)
        {
            if (exception is TaskCanceledException)
            {
                return new JsonResult("Temporarily unavailable: retry the request later")
                {
                    StatusCode = (int?)HttpStatusCode.ServiceUnavailable,
                };
            }
            else if (exception is AuthenticationError authenticationError)
            {
                return new JsonResult(authenticationError.Message)
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized,
                };
            }
            else if (exception is UnknownUserException)
            {
                return new JsonResult("Unknown user")
                {
                    StatusCode = (int?)HttpStatusCode.NotFound,
                };
            }
            else if (exception is UserIsDeletingException)
            {
                return new JsonResult("User is being deleted")
                {
                    StatusCode = (int?)HttpStatusCode.Conflict,
                };
            }
            else if (exception is BackgroundJobIsRunningException)
            {
                return new JsonResult("An operation is already running for this user")
                {
                    StatusCode = (int?)HttpStatusCode.Conflict,
                };
            }
            else if (exception is ParameterRequiredException parameterRequiredException)
            {
                return new JsonResult(parameterRequiredException.Message)
                {
                    StatusCode = (int?)HttpStatusCode.BadRequest,
                };
            }
            else if (exception is ParameterFormatConversionException parameterFormatConversionException)
            {
                return new JsonResult(parameterFormatConversionException.Message)
                {
                    StatusCode = (int?)HttpStatusCode.BadRequest,
                };
            }

            return null;
        }

        public (bool canHandle, TimeSpan? retryAfter) CanHandle(Exception exception)
        {
            if (exception is UserIsDeletingException userIsDeletingException)
            {
                log.LogWarning("Skipping job as user {userId} is deleting", userIsDeletingException.UserId);
                return (canHandle: true, retryAfter: null);
            }
            else if (exception is UnknownUserException unknownUserException)
            {
                log.LogWarning("Skipping job as user {userId} doesn't exist", unknownUserException.UserId);
                return (canHandle: true, retryAfter: null);
            }

            return (canHandle: false, retryAfter: null);
        }
    }
}
