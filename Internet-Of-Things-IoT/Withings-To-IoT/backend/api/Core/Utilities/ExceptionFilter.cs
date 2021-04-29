// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using Microsoft.AspNetCore.Mvc;

    public class ExceptionFilter : IExceptionFilter
    {
        private const int MaxRetries = 3;
        private readonly IReadOnlyCollection<IErrorHandler> handlers;

        public ExceptionFilter(IReadOnlyCollection<IErrorHandler> handlers)
        {
            this.handlers = handlers;
        }

        public async Task<JsonResult> FilterExceptions(Func<Task<JsonResult>> function)
        {
            try
            {
                return await function();
            }
            catch (Exception exception)
            {
                var handled = handlers.Select(handlers => handlers.CanHandleResponse(exception)).FirstOrDefault();

                if (handled == null)
                {
                    throw;
                }

                return handled;
            }
        }

        public async Task FilterExceptions(Func<Task> function)
        {
            for (var i = 0; i < MaxRetries; i++)
            {
                try
                {
                    await function();
                    break;
                }
                catch (Exception exception)
                {
                    var handled = (canHandle: false, retryAfter: (TimeSpan?)null);

                    foreach (var handler in handlers)
                    {
                        var (canHandle, retryAfter) = handler.CanHandle(exception);

                        if (canHandle && !handled.canHandle)
                        {
                            handled.canHandle = true;
                        }

                        if (retryAfter != null && (handled.retryAfter == null || handled.retryAfter.Value < retryAfter.Value))
                        {
                            handled.retryAfter = retryAfter;
                        }
                    }

                    if (!handled.canHandle)
                    {
                        throw;
                    }

                    if (handled.retryAfter == null)
                    {
                        break;
                    }

                    await Task.Delay(handled.retryAfter.Value);
                }
            }
        }
    }
}
