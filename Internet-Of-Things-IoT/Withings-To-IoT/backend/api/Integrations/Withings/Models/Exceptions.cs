// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Models
{
    using H3.Core.Models.Api;

    public class WithingsApiException : ApiException
    {
    }

    public class UnknownWithingsUserException : H3Exception
    {
        public string UserId { get; set; } = null!;
    }
}
