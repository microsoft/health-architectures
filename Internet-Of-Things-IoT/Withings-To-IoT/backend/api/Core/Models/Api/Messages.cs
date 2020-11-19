// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Api
{
    using H3.Core.Models.Fhir;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum ChangeDataFeedOperation
    {
        Create,
        Delete,
    }

    public class StartAccountDeletionMessage
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = null!;
    }

    public class StartDeviceManagementMessage
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = null!;

        [JsonProperty("connectedDevices")]
        public Identifier[] ConnectedDevices { get; set; } = null!;

        [JsonProperty("disconnectedDevices")]
        public Identifier[] DisconnectedDevices { get; set; } = null!;
    }

    public class ChangeDataFeedMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("operation")]
        public ChangeDataFeedOperation Operation { get; set; }

        [JsonProperty("resource")]
        public object Resource { get; set; } = null!;
    }
}
