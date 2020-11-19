// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Core.Models.Api
{
    using System.Collections.Generic;
    using H3.Core.Models.Fhir;
    using Newtonsoft.Json;

    public static class ConsentFactory
    {
        public static Consent WithId(string userId)
        {
            return new Consent
            {
                UserId = userId,
            };
        }
    }

    public static class ConsentExtensions
    {
        public static Consent Verify(this Consent? consent, string userId, bool ignoreFhir = false)
        {
            if (consent == null)
            {
                throw new UnknownUserException
                {
                    UserId = userId,
                };
            }

            if (consent.FhirId == null && !ignoreFhir)
            {
                throw new UnknownUserException
                {
                    UserId = userId,
                };
            }

            if (consent.IsDeleting)
            {
                throw new UserIsDeletingException
                {
                    UserId = userId,
                };
            }

            return consent;
        }
    }

    public class Consent
    {
        [JsonProperty("id")]
        public string UserId { get; set; } = null!;

        [JsonProperty("externalIds")]
        public Dictionary<string, string> ExternalIds { get; set; } = new Dictionary<string, string>();

        [JsonProperty("mobileDevices")]
        public List<MobileDevice> MobileDevices { get; set; } = new List<MobileDevice>();

        [JsonProperty("fhirId")]
        public string FhirId { get; set; } = null!;

        [JsonProperty("isDeleting")]
        public bool IsDeleting { get; set; }

        [JsonProperty("devices")]
        public List<Identifier> Devices { get; set; } = new List<Identifier>();
    }
}
