// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Models
{
    using global::Newtonsoft.Json;

    /// <summary>
    /// Wrapper for all responses from the Withings API.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the response.</typeparam>
    public class Response<T>
        where T : class
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; } = null!;

        [JsonProperty("body")]
        public T Body { get; set; } = null!;
    }

    /// <summary>
    /// A page of Withings measurements.
    /// <a href="http://developer.withings.com/oauth2/#operation/measure-getmeas">See documentation.</a>
    /// </summary>
    public class Measures
    {
        [JsonProperty("updatetime")]
        public long UpdateTime { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; } = null!;

        [JsonProperty("measuregrps")]
        public Group[] MeasureGroups { get; set; } = null!;

        [JsonProperty("more")]
        public int? HasMore { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }
    }

    /// <summary>
    /// A group of Withings measurements.
    /// <a href="http://developer.withings.com/oauth2/#operation/measure-getmeas">See documentation.</a>
    /// </summary>
    public class Group
    {
        [JsonProperty("grpid")]
        public long Id { get; set; }

        [JsonProperty("attrib")]
        public int Attribution { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("category")]
        public int Category { get; set; }

        [JsonProperty("deviceid")]
        public string DeviceID { get; set; } = null!;

        [JsonProperty("measures")]
        public Measure[] Measures { get; set; } = null!;
    }

    /// <summary>
    /// A single Withings measurement.
    /// <a href="http://developer.withings.com/oauth2/#operation/measure-getmeas">See documentation.</a>
    /// </summary>
    public class Measure
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("unit")]
        public int Unit { get; set; }
    }

    /// <summary>
    /// A set of tokens to access the Withings API.
    /// <a href="http://developer.withings.com/oauth2/#operation/oauth2-getaccesstoken">See documentation.</a>
    /// </summary>
    public class Tokens
    {
        [JsonProperty("userid")]
        public string UserId { get; set; } = null!;

        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = null!;

        [JsonProperty("scope")]
        public string Scope { get; set; } = null!;

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = null!;
    }

    /// <summary>
    /// A list of devices associated with a Withings user account.
    /// <a href="http://developer.withings.com/oauth2/#operation/userv2-getdevice">See documentation.</a>
    /// </summary>
    public class Devices
    {
        [JsonProperty("devices")]
        public Device[] Items { get; set; } = null!;
    }

    /// <summary>
    /// A single devices associated with a Withings user account.
    /// <a href="http://developer.withings.com/oauth2/#operation/userv2-getdevice">See documentation.</a>
    /// </summary>
    public class Device
    {
        [JsonProperty("type")]
        public string Type { get; set; } = null!;

        [JsonProperty("battery")]
        public string Battery { get; set; } = null!;

        [JsonProperty("model")]
        public string Model { get; set; } = null!;

        [JsonProperty("model_id")]
        public int ModelID { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; } = null!;

        [JsonProperty("last_session_date")]
        public int LastSessionDate { get; set; }

        [JsonProperty("deviceid")]
        public string DeviceID { get; set; } = null!;
    }
}
