// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.AppCenter;
    using Microsoft.AppCenter.Analytics;
    using Microsoft.AppCenter.Crashes;
    using Microsoft.Identity.Client;
    using Xamarin.Essentials;
    using Xamarin.Forms;
    using Xamarin.Forms.PlatformConfiguration;

    public partial class App : Application
    {
        public const string SCHEME = "myapp";

#pragma warning disable SA1401 // Fields should be private
        public static string B2cAccessToken = string.Empty;
        public static string DeviceId = string.Empty;
#pragma warning restore SA1401 // Fields should be private

        public App()
        {
            Xamarin.Forms.Device.SetFlags(new[] { "Shapes_Experimental", "Brush_Experimental" });

            InitializeComponent();

            AuthenticationClient = PublicClientApplicationBuilder.Create(Constants.ClientId)
                .WithIosKeychainSecurityGroup(Constants.IosKeychainSecurityGroups)
                .WithB2CAuthority(Constants.AuthoritySignin)
                .WithRedirectUri($"msal{Constants.ClientId}://auth")
                .Build();

            if (DeviceInfo.DeviceType == DeviceType.Physical || (DeviceInfo.DeviceType == DeviceType.Virtual && Xamarin.Forms.Device.RuntimePlatform != Xamarin.Forms.Device.iOS))
            {
                // Az PNS
            }

            MainPage = new LoginPage();
        }

        public static IPublicClientApplication AuthenticationClient { get; private set; }

        public static object UIParent { get; set; } = null;

        public static void AddMessage(string message)
        {
            // TODO: decide what to do with message payload.body
            Debug.WriteLine($"AddMessage: {message}");

            Analytics.TrackEvent("PushReceived");
        }

        protected override void OnStart()
        {
            AppCenter.Start(
                string.Format(
                    "ios={0};android={1};",
                    Constants.AppCenterIOSSecret,
                    Constants.AppCenterAndroidSecret),
                typeof(Analytics),
                typeof(Crashes));

            Analytics.TrackEvent("AppStart");
        }

        protected override void OnSleep()
        {
            Analytics.TrackEvent("AppSleep");
        }

        protected override void OnResume()
        {
            Analytics.TrackEvent("AppResume");
        }
    }
}
