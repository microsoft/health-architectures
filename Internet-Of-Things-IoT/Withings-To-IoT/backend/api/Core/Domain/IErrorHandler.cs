// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public interface IErrorHandler
    {
        JsonResult? CanHandleResponse(Exception exception);

        (bool canHandle, TimeSpan? retryAfter) CanHandle(Exception exception);
    }
}
