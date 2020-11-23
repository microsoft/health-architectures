// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    public static class Constants
    {
        public const string ClientId = "1d8cac68-2480-41a2-8862-ddb1b860f857";
        public const string IosKeychainSecurityGroups = "com.microsoft.adalcache";

        public static readonly string[] Scopes = { string.Empty };
        public static readonly string AuthorityBase = $"https://{TenantName}.b2clogin.com/tfp/{TenantId}/";

#pragma warning disable SA1401 // Fields should be private
        public static string WithingsAuthorizeUri = "https://account.withings.com/oauth2_user/authorize2";
        public static string WithingsTokenUri = "https://account.withings.com/oauth2/token";
        public static string WithingsClientId = "5e88175faf8c4e241ab6706b502ca04c08b083b447f1a478fb1e1593d51d24be";

        public static string WithingsRedirectUri = $"{App.SCHEME}://theappp.com";
        public static string WithingsApiUri = "https://wbsapi.withings.net/";
        public static string WithingsScope = "user.info,user.metrics";

        public static string H3BaseEndpoint = "https://h3devapi.azure-api.net/api/";
        public static string H3UserEndpoint = "user";
        public static string H3ObservationsEndpoint = "observations";
        public static string WithingsAuthEndpoint = "withings/auth";

        public static string AppCenterIOSSecret = "8508ca44-c3ba-4970-8d08-f69f24bb193d";
        public static string AppCenterAndroidSecret = "eac53c73-76f4-4fa3-a2a8-6ed448d577b7";
#pragma warning restore SA1401 // Fields should be private

        private const string TenantName = "h3dev";
        private const string TenantId = "h3dev.onmicrosoft.com";
        private const string PolicySignin = "B2C_1_susi";
        private const string PolicyPassword = "B2C_1_reset";

        public static string NotificationChannelName { get; set; } = "Contoso_IoMT_NotifyChannel";

        public static string NotificationHubName { get; set; } = "h3devapi";

        public static string ListenConnectionString { get; set; } = "Endpoint=sb://h3devapinotifications.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=KXV3NwFD7llahtzI58iWRswoWev6zjddCf8bIIs640I=";

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
