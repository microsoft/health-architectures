// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Threading.Tasks;

    public interface IQueue
    {
        Task SendMessage<T>(string queueName, T message)
            where T : class;
    }
}
