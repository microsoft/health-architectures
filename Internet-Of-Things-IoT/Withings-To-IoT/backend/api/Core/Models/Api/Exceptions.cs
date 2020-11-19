// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Api
{
    using System;

    public abstract class H3Exception : Exception
    {
    }

    public class BackgroundJobIsRunningException : H3Exception
    {
    }

    public class UserIsDeletingException : BackgroundJobIsRunningException
    {
        public string UserId { get; set; } = null!;
    }

    public class UnknownUserException : H3Exception
    {
        public string UserId { get; set; } = null!;
    }

    public class AuthenticationError : H3Exception
    {
        public string Problem { get; set; } = null!;

        public override string Message => Problem;
    }

    public class MissingSettingException : H3Exception
    {
        public string Key { get; set; } = null!;

        public override string Message => $"Missing setting '{Key}'";
    }

    public abstract class ApiException : H3Exception
    {
        public string Error { get; set; } = null!;

        public int Status { get; set; }

        public override string Message => $"{Status}: {Error}";
    }

    public class FhirApiException : ApiException
    {
    }
}
