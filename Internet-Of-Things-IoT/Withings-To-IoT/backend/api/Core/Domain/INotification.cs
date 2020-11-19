// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Api;

    public interface INotification
    {
        Task SendNotification(string userId, IReadOnlyCollection<MobileDevice> devices, string message, bool silent, CancellationToken cancellationToken);
    }
}
