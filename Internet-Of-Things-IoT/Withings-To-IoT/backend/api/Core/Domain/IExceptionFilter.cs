// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public interface IExceptionFilter
    {
        Task<JsonResult> FilterExceptions(Func<Task<JsonResult>> function);

        Task FilterExceptions(Func<Task> function);
    }
}
