// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading.Tasks;

    public interface ISwaggerGenerator
    {
        Task<string> GenerateSwagger();
    }
}
