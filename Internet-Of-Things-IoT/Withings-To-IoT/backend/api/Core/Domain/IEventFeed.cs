// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventFeed
    {
        Task SendMessages<T>(IEnumerable<T> messages, CancellationToken cancellationToken)
            where T : class;

        Task DeleteUser(string fhirUserId, CancellationToken cancellationToken);
    }
}
