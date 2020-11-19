// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Api
{
    using System.Runtime.Serialization;
    using H3.Core.Models.Fhir;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public enum MobilePlatform
    {
        [EnumMember(Value = "Android")]
        Android,

        [EnumMember(Value = "iOS")]
        IOS,
    }

    public class UserDto
    {
        /// <summary>
        /// The ids of the devices to connect to the user's account.
        /// </summary>
        /// <remarks>
        /// When a device is connected to a user's account, all historical data
        /// linked to this device is automatically ingested and newly generated
        /// data will be continuously ingested until the device is disconnected
        /// from the user's account.
        /// </remarks>
        [JsonProperty("connectedDeviceIds")]
        public Identifier[]? ConnectedDevices { get; set; }

        /// <summary>
        /// The ids of the devices to disconnect from the user's account.
        /// </summary>
        /// <remarks>
        /// When a device is disconnected from a user's account, all data ingested
        /// from this device is automatically deleted. This operation can't be undone.
        /// </remarks>
        [JsonProperty("disconnectedDeviceIds")]
        public Identifier[]? DisconnectedDevices { get; set; }

        /// <summary>
        /// The mobile device to link to the user's account.
        /// </summary>
        [JsonProperty("mobileDevice")]
        public MobileDevice? MobileDevice { get; set; }
    }

    public class MobileDevice
    {
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("devicePlatform")]
        public MobilePlatform Platform { get; set; }

        [JsonRequired]
        [JsonProperty("deviceId")]
        public string Id { get; set; } = null!;
    }
}
