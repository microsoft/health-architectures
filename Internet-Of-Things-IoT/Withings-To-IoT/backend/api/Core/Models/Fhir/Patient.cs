// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Fhir
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// A FHIR Patient resource.
    /// <a href="https://www.hl7.org/fhir/R4/patient.html">See documentation.</a>
    /// </summary>
    public class Patient : IHasId
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; } = null!;

        [JsonProperty("name")]
        public Name[] Names { get; set; } = Array.Empty<Name>();
    }

    /// <summary>
    /// A FHIR HumanName instance.
    /// <a href="https://www.hl7.org/fhir/datatypes.html#humanname">See documentation.</a>
    /// </summary>
    public class Name
    {
        [JsonProperty("family")]
        public string FamilyName { get; set; } = null!;

        [JsonProperty("given")]
        public string[] GivenNames { get; set; } = Array.Empty<string>();
    }
}
