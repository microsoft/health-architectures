// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Fhir
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// A FHIR resource collection or set of resource instructions.
    /// <a href="https://www.hl7.org/fhir/bundle.html">See documentation.</a>
    /// </summary>
    /// <typeparam name="T">The type of FHIR resource included in the collection.</typeparam>
    public class Bundle<T>
        where T : class
    {
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; } = null!;

        [JsonProperty("link")]
        public Link[] Links { get; set; } = Array.Empty<Link>();

        [JsonProperty("type")]
        public string Type { get; set; } = null!;

        [JsonProperty("entry")]
        public BundleEntry<T>[] Entries { get; set; } = Array.Empty<BundleEntry<T>>();
    }

    /// <summary>
    /// A FHIR pagination link.
    /// <a href="https://www.hl7.org/fhir/http.html#paging">See documentation.</a>
    /// </summary>
    public class Link
    {
        [JsonProperty("relation")]
        public string Relation { get; set; } = null!;

        [JsonProperty("url")]
        public string Url { get; set; } = null!;
    }

    /// <summary>
    /// An entry in the FHIR resource collection.
    /// <a href="https://www.hl7.org/fhir/bundle.html">See documentation.</a>
    /// </summary>
    /// <typeparam name="T">The type of FHIR resource included in the collection.</typeparam>
    public class BundleEntry<T>
        where T : class
    {
        [JsonProperty("resource")]
        public T Resource { get; set; } = null!;

        [JsonProperty("request")]
        public BundleRequest Request { get; set; } = null!;

        [JsonProperty("response")]
        public BundleResponse Response { get; set; } = null!;
    }

    /// <summary>
    /// Additional execution information for how to handle the resource instruction.
    /// <a href="https://www.hl7.org/fhir/bundle.html">See documentation.</a>
    /// </summary>
    public class BundleRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; } = null!;

        [JsonProperty("url")]
        public string Url { get; set; } = null!;
    }

    /// <summary>
    /// Results of the resource instruction execution.
    /// <a href="https://www.hl7.org/fhir/bundle.html">See documentation.</a>
    /// </summary>
    public class BundleResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; } = null!;

        [JsonProperty("etag")]
        public string? ETag { get; set; }
    }
}
