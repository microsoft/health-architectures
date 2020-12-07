// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    public static class Constants
    {
        public const string ClientId = "[REPLACE_ME]";
        public const string IosKeychainSecurityGroups = "com.microsoft.adalcache";

        public static readonly string[] Scopes = { string.Empty };
        public static readonly string AuthorityBase = $"https://{TenantName}.b2clogin.com/tfp/{TenantId}/";

#pragma warning disable SA1401 // Fields should be private
        public static string WithingsAuthorizeUri = "https://account.withings.com/oauth2_user/authorize2";
        public static string WithingsTokenUri = "https://account.withings.com/oauth2/token";
        public static string WithingsClientId = "[REPLACE_ME]";

        public static string WithingsRedirectUri = $"{App.SCHEME}://theappp.com";
        public static string WithingsApiUri = "https://wbsapi.withings.net/";
        public static string WithingsScope = "user.info,user.metrics";

        public static string H3BaseEndpoint = "[REPLACE_ME]";
        public static string H3UserEndpoint = "user";
        public static string H3ObservationsEndpoint = "observations";
        public static string WithingsAuthEndpoint = "withings/auth";

        public static string AppCenterIOSSecret = "[REPLACE_ME]";
        public static string AppCenterAndroidSecret = "[REPLACE_ME]";
#pragma warning restore SA1401 // Fields should be private

        private const string TenantName = "[REPLACE_ME]";
        private const string TenantId = "[REPLACE_ME]";
        private const string PolicySignin = "B2C_1_susi";
        private const string PolicyPassword = "B2C_1_reset";

        public static string NotificationChannelName { get; set; } = "Contoso_IoMT_NotifyChannel";

        public static string NotificationHubName { get; set; } = "[REPLACE_ME]";

        public static string ListenConnectionString { get; set; } = "[REPLACE_ME]";

        public static string DebugTag { get; set; } = "Contoso_IoMT_Notify";

        public static string[] SubscriptionTags { get; set; } = { "default" };

        public static string FCMTemplateBody { get; set; } = "{\"data\":{\"message\":\"$(messageParam)\"}}";

        public static string APNTemplateBody { get; set; } = "{\"aps\":{\"alert\":\"$(messageParam)\"}}";

        public static string AuthoritySignin => $"{AuthorityBase}{PolicySignin}";

        public static string AuthorityPasswordReset => $"{AuthorityBase}{PolicyPassword}";

        public class Manufacturers
        {
            private Manufacturers(string value)
            {
                Value = value;
            }

            public static Manufacturers Withings
            {
                get { return new Manufacturers("http://withings.com"); }
            }

            public static Manufacturers Propeller
            {
                get { return new Manufacturers("propeller"); }
            }

            public static Manufacturers EMFIT
            {
                get { return new Manufacturers("emfit"); }
            }

            public static Manufacturers Garmin
            {
                get { return new Manufacturers("garmin"); }
            }

            public string Value { get; set; }
        }
    }
}
