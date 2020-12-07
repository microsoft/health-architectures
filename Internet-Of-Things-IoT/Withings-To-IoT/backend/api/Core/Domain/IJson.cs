// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    public interface IJson
    {
        T Load<T>(string value)
            where T : class;

        string Dump<T>(T value)
            where T : class;
    }
}
