// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Api
{
    using System;
    using H3.Core.Models.Fhir;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class User
    {
        /// <summary>
        /// The list of devices connected to the user's account.
        /// </summary>
        /// <remarks>
        /// Observations are only ingested and connected to the user's account
        /// for the devices included in this list.
        /// </remarks>
        [JsonProperty("connectedDevices")]
        public Ref[] ConnectedDevices { get; set; } = Array.Empty<Ref>();

        /// <summary>
        /// The list of devices available to connect to the user's account.
        /// </summary>
        [JsonProperty("disconnectedDevices")]
        public Ref[] DisconnectedDevices { get; set; } = Array.Empty<Ref>();

        /// <summary>
        /// Id of the long running operation modifying the user's devices.
        /// </summary>
        [JsonProperty("jobId", NullValueHandling = NullValueHandling.Ignore)]
        public string? JobId { get; set; }
    }

    public class UserDeletion
    {
        /// <summary>
        /// Id of the long running operation deleting the user's account.
        /// </summary>
        [JsonProperty("jobId", NullValueHandling = NullValueHandling.Ignore)]
        public string? JobId { get; set; }
    }

    public class JobStatus
    {
        /// <summary>
        /// The status of the long running operation.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public OrchestrationRuntimeStatus Status { get; set; }
    }
}
