// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Models
{
    using global::Newtonsoft.Json;

    public class CallbackReceivedMessage
    {
        [JsonProperty("withingsUserId")]
        public int WithingsUserId { get; set; }

        [JsonProperty("startDateEpoch")]
        public long StartDateEpoch { get; set; }

        [JsonProperty("endDateEpoch")]
        public long EndDateEpoch { get; set; }
    }

    public class StartNotificationIngestionMessage
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = null!;

        [JsonProperty("startDateEpoch")]
        public long StartDateEpoch { get; set; }

        [JsonProperty("endDateEpoch")]
        public long EndDateEpoch { get; set; }
    }

    public class StartDeviceIngestionMessage
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = null!;

        [JsonProperty("withingsDeviceIds")]
        public string[] WithingsDeviceIds { get; set; } = null!;
    }

    public class StartDeviceDeletionMessage
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = null!;

        [JsonProperty("withingsDeviceIds")]
        public string[] WithingsDeviceIds { get; set; } = null!;
    }
}
