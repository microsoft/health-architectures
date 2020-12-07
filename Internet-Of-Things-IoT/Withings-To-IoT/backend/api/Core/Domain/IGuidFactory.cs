// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;

    public interface IGuidFactory
    {
        Guid Create(params string[] values);
    }
}
