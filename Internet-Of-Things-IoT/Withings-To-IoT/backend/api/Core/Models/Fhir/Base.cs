// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Fhir
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Marker interface for all FHIR resources.
    /// </summary>
    public interface IHasId
    {
        string Id { get; }

        string ResourceType { get; }
    }

    /// <summary>
    /// A FHIR CodableConcept item.
    /// <a href="https://www.hl7.org/fhir/datatypes.html#codeableconcept">See documentation.</a>
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Code
    {
        [JsonProperty("coding")]
        public Coding[] Coding { get; set; } = null!;

        [JsonProperty("text")]
        public string? Text { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Code code &&
                   Coding.Length == code.Coding.Length &&
                   Coding.Zip(code.Coding).All(tuple => EqualityComparer<Coding>.Default.Equals(tuple.First, tuple.Second));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 0;

                foreach (var code in Coding)
                {
                    hashCode = (hashCode * 31) ^ code.GetHashCode();
                }

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A FHIR Coding item.
    /// <a href="https://www.hl7.org/fhir/datatypes.html#coding">See documentation.</a>
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Coding
    {
        [JsonProperty("system")]
        public string System { get; set; } = null!;

        [JsonProperty("code")]
        public string Code { get; set; } = null!;

        [JsonProperty("display")]
        public string Display { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            return obj is Coding coding &&
                   System == coding.System &&
                   Code == coding.Code;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(System, Code);
        }
    }

    /// <summary>
    /// A FHIR Reference item.
    /// <a href="https://www.hl7.org/fhir/references.html">See documentation.</a>
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Ref
    {
        [JsonProperty("reference")]
        public string Reference { get; set; } = null!;

        [JsonProperty("identifier")]
        public Identifier Identifier { get; set; } = null!;

        [JsonProperty("display")]
        public string Display { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            return obj is Ref @ref &&
                   Reference == @ref.Reference &&
                   EqualityComparer<Identifier>.Default.Equals(Identifier, @ref.Identifier);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Reference, Identifier);
        }
    }

    /// <summary>
    /// A FHIR Identifier item.
    /// <a href="https://www.hl7.org/fhir/datatypes.html#identifier">See documentation.</a>
    /// </summary>
    public class Identifier
    {
        [JsonProperty("system")]
        public string System { get; set; } = null!;

        [JsonProperty("value")]
        public string Value { get; set; } = null!;

        [JsonProperty("type")]
        public Code Type { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            return obj is Identifier identifier &&
                   System == identifier.System &&
                   Value == identifier.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(System, Value);
        }
    }
}
