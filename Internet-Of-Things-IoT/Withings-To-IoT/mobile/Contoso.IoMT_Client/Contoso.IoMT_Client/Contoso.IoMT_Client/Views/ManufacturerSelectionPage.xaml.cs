// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Contoso.IoMT_Client.Controls;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManufacturerSelectionPage : ContentPage
    {
        public static readonly BindableProperty SupportedDevicesProperty =
            BindableProperty.Create("SupportedDevices", typeof(ObservableCollection<object>), typeof(ManufacturerSelectionPage), null);

        public static readonly BindableProperty FutureDevicesProperty =
            BindableProperty.Create("FutureDevices", typeof(ObservableCollection<object>), typeof(ManufacturerSelectionPage), null);

        public static readonly BindableProperty DeviceInfoProperty =
            BindableProperty.Create("DeviceInfo", typeof(Models.Device), typeof(ManufacturerSelectionPage), null);

        private static Random random = new Random();
        private Models.Device selectedDevice = null;

        public ManufacturerSelectionPage()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }

            InitializeComponent();
        }

        public ObservableCollection<object> SupportedDevices
        {
            get { return (ObservableCollection<object>)GetValue(SupportedDevicesProperty); }
            set { SetValue(SupportedDevicesProperty, value); }
        }

        public ObservableCollection<object> FutureDevices
        {
            get { return (ObservableCollection<object>)GetValue(FutureDevicesProperty); }
            set { SetValue(FutureDevicesProperty, value); }
        }

        public Models.Device DeviceInfo
        {
            get { return (Models.Device)GetValue(DeviceInfoProperty); }
            set { SetValue(DeviceInfoProperty, value); }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override void OnAppearing()
        {
            DataTemplate template = (DataTemplate)this.Resources["withingsDeviceDataTemplate"];

            SupportedDevices = new ObservableCollection<object>();
            GenerateSupportedDevicesCollection(SupportedDevices);

            supportedDevicesWrapLayout.Children.Clear();
            foreach (var supportedDevice in SupportedDevices)
            {
                Controls.ContentPresenter contentPresenter = BuildContentPresenter(template, (Models.Device)supportedDevice);
                supportedDevicesWrapLayout.Children.Add(contentPresenter);
            }

            FutureDevices = new ObservableCollection<object>();
            GenerateFutureDevicesCollection(FutureDevices);

            futureDevicesWrapLayout.Children.Clear();
            foreach (var futureDevice in FutureDevices)
            {
                Controls.ContentPresenter contentPresenter = BuildContentPresenter(template, (Models.Device)futureDevice);
                futureDevicesWrapLayout.Children.Add(contentPresenter);
            }

            base.OnAppearing();
        }

        private Controls.ContentPresenter BuildContentPresenter(DataTemplate template, Models.Device supportedDevice)
        {
            Controls.ContentPresenter contentPresenter = new Controls.ContentPresenter()
            {
                BindingContext = supportedDevice,
            };
            contentPresenter.ItemTemplate = template;
            contentPresenter.WidthRequest = 160;
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            contentPresenter.GestureRecognizers.Add(tapGestureRecognizer);
            return contentPresenter;
        }

        private void GenerateFutureDevicesCollection(ObservableCollection<object> futureDevices)
        {
            // TODO
        }

        private void GenerateSupportedDevicesCollection(ObservableCollection<object> supportedDevices)
        {
            Models.Device device = new Models.Device()
            {
                Display = "BPM Connect (Blood Pressure Monitor)",
                Identifier = new Models.Identifier()
                {
                    System = "http://withings.com",
                    Type = new Models.Type()
                    {
                        Coding = new List<Models.Coding>()
                        {
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/model_id"),
                                Code = "45",
                            },
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/type"),
                                Code = "Blood Pressure Monitor",
                            },
                        },
                    },
                },
                IsConnected = false,
                IsSelected = false,
                ShowExtraInfo = true,
            };
            supportedDevices.Add(device);

            device = new Models.Device()
            {
                Display = "Body (Scale)",
                Identifier = new Models.Identifier()
                {
                    System = "http://withings.com",
                    Type = new Models.Type()
                    {
                        Coding = new List<Models.Coding>()
                        {
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/model_id"),
                                Code = "7",
                            },
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/type"),
                                Code = "Scale",
                            },
                        },
                    },
                },
                IsConnected = false,
                IsSelected = false,
                ShowExtraInfo = true,
            };
            supportedDevices.Add(device);

            device = new Models.Device()
            {
                Display = "Thermo Smart Temporal",
                Identifier = new Models.Identifier()
                {
                    System = "http://withings.com",
                    Type = new Models.Type()
                    {
                        Coding = new List<Models.Coding>()
                        {
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/model_id"),
                                Code = "123",
                            },
                            new Models.Coding()
                            {
                                System = new Uri("http://withings.com/device/type"),
                                Code = "Thremo",
                            },
                        },
                    },
                },
                IsConnected = false,
                IsSelected = false,
                ShowExtraInfo = true,
            };
            supportedDevices.Add(device);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Controls.ContentPresenter contentPresenter = (Controls.ContentPresenter)sender;
            Models.Device device = (Models.Device)contentPresenter.BindingContext;

            if (selectedDevice != null)
            {
                string selectedModelId = (from c in selectedDevice.Identifier.Type.Coding
                                          where c.System.AbsoluteUri == $"{selectedDevice.Identifier.System}/device/model_id"
                                          select c.Code).FirstOrDefault();
                string currentModelId = (from c in device.Identifier.Type.Coding
                                         where c.System.AbsoluteUri == $"{device.Identifier.System}/device/model_id"
                                         select c.Code).FirstOrDefault();

                if (selectedModelId != currentModelId)
                {
                    selectedDevice.IsSelected = false;
                }
            }

            device.IsSelected = !device.IsSelected;

            selectedDevice = device.IsSelected ? device : null;

            authenticateWithManufacturer.IsEnabled = (selectedDevice != null) ? true : false;

            if (selectedDevice != null && Device.Idiom == TargetIdiom.Tablet)
            {
                DeviceInfo = new Models.Device()
                {
                    Display = selectedDevice.Display,
                    Identifier = selectedDevice.Identifier,
                    ShowExtraInfo = true,
                    IsConnected = false,
                    IsSelected = false,
                };

                authenticateWithManufacturer.IsVisible = true;
            }
            else
            {
                DeviceInfo = null;

                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    authenticateWithManufacturer.IsVisible = false;
                }
            }

            deviceDetails.BindingContext = DeviceInfo;
            deviceContentView.IsVisible = (selectedDevice == null) ? false : true;
        }

        private async void AuthenticateWithManufacturer_Clicked(object sender, EventArgs e)
        {
            if (selectedDevice.Identifier.System.ToLower() == Constants.Manufacturers.Withings.Value)
            {
                var res = await GetWithingsAuthCode();

                if (res.Length > 0)
                {
                    HttpClient httpClient = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, Constants.H3BaseEndpoint + Constants.WithingsAuthEndpoint)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new
                        {
                            withingsAccessCode = res,
                            withingsRedirectUri = Constants.WithingsRedirectUri,
                        })),
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", App.B2cAccessToken);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var result = await httpClient.SendAsync(request);

                    if (result.IsSuccessStatusCode)
                    {
                        List<object> connectedDevices = new List<object>();
                        List<object> disconnectedDevices = new List<object>();

                        var r = await Services.WithingsService.GetDevices(connectedDevices, disconnectedDevices);
                        if (r == System.Net.HttpStatusCode.OK)
                        {
                            App.Current.MainPage = new NavigationPage(new DeviceManagementPage(connectedDevices, disconnectedDevices, true));
                        }
                        else if (r == System.Net.HttpStatusCode.Conflict)
                        {
                            await DisplayAlert("Conflict", "PApp.Current.MainPage = new NavigationPagerevious user delete operation is in progress. Please try again later", "Ok");
                        }
                        else
                        {
                            await DisplayAlert("Withing auth failed", "Something went wrong. Please try again", "Ok");
                        }
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        await DisplayAlert("Conflict", "Previous user delete operation is in progress. Please try again later", "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Ingestion failed", "Something went wrong. Please try again", "Ok");
                    }
                }
                else
                {
                    await DisplayAlert("Withing auth failed", "Something went wrong. Please try again", "Ok");
                }
            }
            else
            {
                await DisplayAlert("Coming soon", "Selected device is not supported yet", "Ok");
            }

            authenticateWithManufacturer.IsEnabled = false;
            selectedDevice.IsSelected = false;
            selectedDevice = null;
            deviceContentView.IsVisible = false;

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                authenticateWithManufacturer.IsVisible = false;
            }
        }

        private async Task<string> GetWithingsAuthCode()
        {
            int length = random.Next(5, 16);
            string withingsState = RandomString(length);

            string url = $"{Constants.WithingsAuthorizeUri}?response_type=code&client_id={Constants.WithingsClientId}&state='{withingsState}'&scope={Constants.WithingsScope}&redirect_uri={Constants.WithingsRedirectUri}&b=authorize2";

            // Request auth code
            var authResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(
                new Uri(url),
                new Uri($"{App.SCHEME}://"));

            // Extract the code
            string code = authResult?.Properties["code"];
            string state = authResult?.Properties["state"];

            if (state.CompareTo(withingsState) == 1)
            {
                return code;
            }
            else
            {
                return string.Empty;
            }
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
