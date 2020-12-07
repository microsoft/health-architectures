// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Fhir
{
    using Newtonsoft.Json;

    /// <summary>
    /// A FHIR Observation resource.
    /// <a href="https://www.hl7.org/fhir/observation.html">See documentation.</a>
    /// </summary>
    public class Observation : IHasId
    {
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; } = null!;

        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        [JsonProperty("status")]
        public string Status { get; set; } = null!;

        [JsonProperty("category")]
        public Code[] Category { get; set; } = null!;

        [JsonProperty("code")]
        public Code Code { get; set; } = null!;

        [JsonProperty("subject")]
        public Ref Subject { get; set; } = null!;

        [JsonProperty("encounter")]
        public Ref Encounter { get; set; } = null!;

        [JsonProperty("effectiveDateTime")]
        public string EffectiveDateTime { get; set; } = null!;

        [JsonProperty("issued")]
        public string Issued { get; set; } = null!;

        [JsonProperty("valueQuantity")]
        public Quantity ValueQuantity { get; set; } = null!;

        [JsonProperty("device")]
        public Ref Device { get; set; } = null!;
    }

    /// <summary>
    /// A FHIR quantity instance.
    /// <a href="https://www.hl7.org/fhir/datatypes.html#quantity">See documentation.</a>
    /// </summary>
    public class Quantity
    {
        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = null!;

        [JsonProperty("system")]
        public string System { get; set; } = null!;

        [JsonProperty("code")]
        public string Code { get; set; } = null!;
    }
}
