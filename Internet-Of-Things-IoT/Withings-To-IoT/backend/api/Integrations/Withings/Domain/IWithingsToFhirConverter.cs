// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Collections.Generic;
    using H3.Core.Models.Fhir;
    using H3.Integrations.Withings.Models;

    /// <summary>
    /// This interface should not be referenced outside of the <see cref="Withings"/> namespace.
    /// </summary>
    public interface IWithingsToFhirConverter
    {
        string System { get; }

        IEnumerable<Ref> Convert(IEnumerable<Device> devices, IReadOnlyCollection<string>? withingsDeviceIds);

        IEnumerable<Observation> Convert(string fhirUserId, IEnumerable<Group> measureGroups, IReadOnlyCollection<Ref> withingsDevices);

        Func<Observation, bool> ShouldDelete(IReadOnlyCollection<string> deviceIds);
    }
}
