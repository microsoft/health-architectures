// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AppCenter.Analytics;
    using Microsoft.Identity.Client;
    using Newtonsoft.Json.Linq;
    using Xamarin.Forms;

    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class LoginPage : ContentPage
    {
        private bool isAuthenticated = false;
        private List<object> connectedWithingDevices = new List<object>();
        private List<object> disconnectedWithingDevices = new List<object>();
        private bool autoLogin = true;

        public LoginPage(bool autoLogin = true)
        {
            this.autoLogin = autoLogin;

            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (autoLogin)
            {
                try
                {
                    // Look for existing account
                    IEnumerable<IAccount> accounts = await App.AuthenticationClient.GetAccountsAsync();

                    if (accounts.Count() > 0)
                    {
                        var account = accounts.FirstOrDefault();
                        AuthenticationResult authenticationResult = await App.AuthenticationClient.AcquireTokenSilent(Constants.Scopes, accounts.First()).ExecuteAsync();

                        if (authenticationResult != null)
                        {
                            isAuthenticated = true;

                            Analytics.TrackEvent("SilentAuthenticationSuccessful");

                            App.B2cAccessToken = authenticationResult.IdToken;
                            UpdateButtons(!isAuthenticated);

                            var result = await Services.WithingsService.GetDevices(connectedWithingDevices, disconnectedWithingDevices);
                            if (result == System.Net.HttpStatusCode.OK)
                            {
                                if (connectedWithingDevices.Count > 0)
                                {
                                    App.Current.MainPage = new NavigationPage(new DashboardPage())
                                    {
                                        Style = (Style)Application.Current.Resources["appNavigation"],
                                    };
                                }
                                else
                                {
                                    await Navigation.PushModalAsync(new ManufacturerSelectionPage());
                                }
                            }
                            else if (result == System.Net.HttpStatusCode.NotFound)
                            {
                                await Navigation.PushModalAsync(new ManufacturerSelectionPage());
                            }
                            else if (result == System.Net.HttpStatusCode.Conflict)
                            {
                                await DisplayAlert("Conflict", "Previous user delete operation is in progress. Please try again later", "Ok");
                                UpdateButtons(true);
                            }
                            else
                            {
                                await DisplayAlert("Error", "Something went wrong. Please try again", "Ok");
                                Console.WriteLine(result.ToString());
                                UpdateButtons(true);
                            }
                        }
                    }
                }
                catch
                {
                    // Do nothing - the user isn't logged in
                    UpdateButtons(true);
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (!isAuthenticated)
            {
                return true;
            }

            return base.OnBackButtonPressed();
        }

        private void UpdateButtons(bool enableLoginButton)
        {
            b2cLoginButton.IsEnabled = enableLoginButton;
            activityIndicator.IsRunning = !enableLoginButton;
            activityIndicator.IsVisible = !enableLoginButton;
        }

        private async void OnB2CLoginButtonClicked(object sender, EventArgs e)
        {
            AuthenticationResult authenticationResult;
            try
            {
                authenticationResult = await App.AuthenticationClient
                    .AcquireTokenInteractive(Constants.Scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .WithParentActivityOrWindow(App.UIParent)
                    .ExecuteAsync();

                string messageText = string.Empty;
                if (authenticationResult != null)
                {
                    isAuthenticated = true;

                    Analytics.TrackEvent("AuthenticationSuccessful");

                    App.B2cAccessToken = authenticationResult.IdToken;
                    await Services.WithingsService.UpdateMobileDeviceIdAsync(App.DeviceId);

                    if (authenticationResult.Account.Username != "unknown" && !string.IsNullOrEmpty(authenticationResult.Account.Username))
                    {
                        messageText = string.Format("Welcome {0}", authenticationResult.Account.Username);
                    }
                    else
                    {
                        messageText = string.Format("UserId: {0}", authenticationResult.Account.HomeAccountId);
                    }

                    UpdateButtons(!isAuthenticated);

                    var result = await Services.WithingsService.GetDevices(connectedWithingDevices, disconnectedWithingDevices);
                    if (result == System.Net.HttpStatusCode.OK)
                    {
                        if (connectedWithingDevices.Count > 0)
                        {
                            App.Current.MainPage = new NavigationPage(new DashboardPage())
                            {
                                Style = (Style)Application.Current.Resources["appNavigation"],
                            };
                        }
                        else
                        {
                            await Navigation.PushModalAsync(new ManufacturerSelectionPage());
                        }
                    }
                    else if (result == System.Net.HttpStatusCode.NotFound)
                    {
                        await Navigation.PushModalAsync(new ManufacturerSelectionPage());
                    }
                    else if (result == System.Net.HttpStatusCode.Conflict)
                    {
                        await DisplayAlert("Conflict", "Previous user delete operation is in progress. Please try again later", "Ok");
                        UpdateButtons(true);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Something went wrong. Please try again", "Ok");
                        Console.WriteLine(result.ToString());
                        UpdateButtons(true);
                    }
                }
                else
                {
                    Analytics.TrackEvent("AuthenticationFailed");

                    await DisplayAlert("B2C auth failed", "Something went wrong. Please try again", "Ok");
                    UpdateButtons(true);
                }
            }
            catch (MsalException ex)
            {
                if (ex.Message != null && ex.Message.Contains("AADB2C90118"))
                {
                    // TODO: decide what to do on forgotten password
                }
                else if (ex.ErrorCode != "authentication_canceled")
                {
                    await DisplayAlert("An error has occurred", "Exception message: " + ex.Message, "Dismiss");
                }

                UpdateButtons(true);
            }
        }
    }
}
