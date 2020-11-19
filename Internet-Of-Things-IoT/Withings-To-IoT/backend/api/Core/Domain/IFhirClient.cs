// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Models.Fhir;

    public interface IFhirClient
    {
        Task<IReadOnlyCollection<Observation>> CreateObservations(string userId, IReadOnlyCollection<Observation> observations, CancellationToken cancellationToken);

        Task DeleteObservations(string userId, string fhirUserId, Func<Observation, bool> shouldDelete, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<Observation>> FetchObservations(string userId, string fhirUserId, DateTimeOffset? after, DateTimeOffset? before, CancellationToken cancellationToken);

        Task<Patient> CreatePatient(string userId, string? familyName, string? givenName, CancellationToken cancellationToken);

        Task DeletePatient(string userId, string fhirUserId, CancellationToken cancellationToken);
    }
}
