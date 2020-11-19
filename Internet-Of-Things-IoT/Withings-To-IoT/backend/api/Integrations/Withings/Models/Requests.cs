// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Models
{
    using global::Newtonsoft.Json;

    public class AuthDto
    {
        /// <summary>
        /// The access code returned by the Withings authentication process.
        /// </summary>
        [JsonRequired]
        [JsonProperty("withingsAccessCode")]
        public string WithingsAccessCode { get; set; } = null!;

        /// <summary>
        /// The redirect URI used during the Withings authentication process.
        /// </summary>
        [JsonRequired]
        [JsonProperty("withingsRedirectUri")]
        public string WithingsRedirectUri { get; set; } = null!;
    }
}
