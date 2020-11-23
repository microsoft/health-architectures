// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceManagementPage : ContentPage
    {
        public static readonly BindableProperty ConnectedDevicesProperty =
            BindableProperty.Create("ConnectedDevices", typeof(ObservableCollection<object>), typeof(DeviceManagementPage), null);

        public static readonly BindableProperty DisconnectedDevicesProperty =
            BindableProperty.Create("DisconnectedDevices", typeof(ObservableCollection<object>), typeof(DeviceManagementPage), null);

        public static readonly BindableProperty DeviceInfoProperty =
            BindableProperty.Create("DeviceInfo", typeof(Models.Device), typeof(DeviceManagementPage), null);

        private bool isConnectedDeviceSelected = false;
        private Models.Device selectedDevice = null;
        private bool firstConnect = false;

        public DeviceManagementPage(List<object> connectedWithingDevices, List<object> disconnectedWithingDevices, bool firstConnect = false)
        {
            this.firstConnect = firstConnect;
            if (Device.RuntimePlatform == Device.Android)
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }

            ConnectedDevices = new ObservableCollection<object>(connectedWithingDevices);
            DisconnectedDevices = new ObservableCollection<object>(disconnectedWithingDevices);

            InitializeComponent();
        }

        public ObservableCollection<object> ConnectedDevices
        {
            get { return (ObservableCollection<object>)GetValue(ConnectedDevicesProperty); }
            set { SetValue(ConnectedDevicesProperty, value); }
        }

        public ObservableCollection<object> DisconnectedDevices
        {
            get { return (ObservableCollection<object>)GetValue(DisconnectedDevicesProperty); }
            set { SetValue(DisconnectedDevicesProperty, value); }
        }

        public Models.Device DeviceInfo
        {
            get { return (Models.Device)GetValue(DeviceInfoProperty); }
            set { SetValue(DeviceInfoProperty, value); }
        }

        protected override bool OnBackButtonPressed()
        {
            if (firstConnect)
            {
                App.Current.MainPage = new NavigationPage(new DashboardPage());
            }

            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            DataTemplate template = (DataTemplate)this.Resources["withingsDeviceDataTemplate"];

            connectedDevicesWrapLayout.Children.Clear();
            foreach (var connectedItem in ConnectedDevices)
            {
                Controls.ContentPresenter contentPresenter = BuildContentPresenter(template, connectedItem, true);
                connectedDevicesWrapLayout.Children.Add(contentPresenter);
            }

            disconnectedDevicesWrapLayout.Children.Clear();
            foreach (var disconnectedItem in DisconnectedDevices)
            {
                Controls.ContentPresenter contentPresenter = BuildContentPresenter(template, disconnectedItem, false, true);
                disconnectedDevicesWrapLayout.Children.Add(contentPresenter);
            }

            if (firstConnect || Device.RuntimePlatform == Device.iOS)
            {
                backtoDashboradButton.IsVisible = true;
            }

            base.OnAppearing();
        }

        private Controls.ContentPresenter BuildContentPresenter(DataTemplate template, object connectedItem, bool isConnected, bool showExtraInfo = false)
        {
            Models.Device device = JsonConvert.DeserializeObject<Models.Device>(connectedItem.ToString());
            device.IsConnected = isConnected;
            device.ShowExtraInfo = showExtraInfo;

            Controls.ContentPresenter contentPresenter = new Controls.ContentPresenter()
            {
                BindingContext = device,
            };
            contentPresenter.ItemTemplate = template;
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            contentPresenter.GestureRecognizers.Add(tapGestureRecognizer);
            return contentPresenter;
        }

        private async void ConnectDisconnectDevice(object sender, EventArgs e)
        {
            HttpResponseMessage result;
            if (isConnectedDeviceSelected)
            {
                result = await Services.WithingsService.DisconnectDeviceAsync(selectedDevice.Identifier.System, selectedDevice.Identifier.Value);
            }
            else
            {
                result = await Services.WithingsService.ConnectDeviceAsync(selectedDevice.Identifier.System, selectedDevice.Identifier.Value);
            }

            if (result.IsSuccessStatusCode)
            {
                List<object> connectedDevices = new List<object>();
                List<object> disconnectedDevices = new List<object>();

                var res = await Services.WithingsService.GetDevices(connectedDevices, disconnectedDevices);
                if (res == System.Net.HttpStatusCode.OK)
                {
                    ConnectedDevices.Clear();
                    DisconnectedDevices.Clear();
                    ConnectedDevices = new ObservableCollection<object>(connectedDevices);
                    DisconnectedDevices = new ObservableCollection<object>(disconnectedDevices);
                }
                else if (res == System.Net.HttpStatusCode.Conflict)
                {
                    await DisplayAlert("Conflict", "Previous user delete operation is in progress. Please try again later", "Ok");
                }
                else
                {
                    await DisplayAlert("Error", "Something went wrong. Please try again", "Ok");
                }
            }
            else
            {
                await DisplayAlert("Withing auth failed", "Something went wrong. Please try again", "Ok");
            }

            OnAppearing();

            if (result.IsSuccessStatusCode)
            {
                actionButton.IsEnabled = false;
                actionButton.Text = "Select Device";
                selectedDevice.IsSelected = false;
                selectedDevice = null;
                deviceContentView.IsVisible = false;
            }

            if (Device.Idiom == TargetIdiom.Tablet)
                {
                    actionButton.IsVisible = false;
                }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Controls.ContentPresenter contentPresenter = (Controls.ContentPresenter)sender;
            Models.Device device = (Models.Device)contentPresenter.BindingContext;
            isConnectedDeviceSelected = device.IsConnected;

            if (selectedDevice != null && selectedDevice.Identifier.Value != device.Identifier.Value)
            {
                selectedDevice.IsSelected = false;
            }

            device.IsSelected = !device.IsSelected;

            selectedDevice = device.IsSelected ? device : null;

            actionButton.Text = isConnectedDeviceSelected ? "Disconnect Device" : "Connect Device";
            actionButton.IsEnabled = (selectedDevice != null) ? true : false;

            if (!actionButton.IsEnabled)
            {
                actionButton.Text = "Select Device";
            }

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

                actionButton.IsVisible = true;
            }
            else
            {
                DeviceInfo = null;

                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    actionButton.IsVisible = false;
                }
            }

            deviceDetails.BindingContext = DeviceInfo;
            deviceContentView.IsVisible = (selectedDevice == null) ? false : true;
        }

        private async void BacktoDashboradButton_Clicked(object sender, EventArgs e)
        {
            if (!firstConnect)
            {
                await Navigation.PopAsync();
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new DashboardPage());
            }
        }

        private async void OnDisconnectTapped(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Disconnect Withings account?", "If you disconnect your Withings account, all Withings devices will be removed.", "Disconnect Withings account", "Not now");

            if (answer)
            {
                var result = await Services.WithingsService.DisconnectAccountAsync();

                if (result.IsSuccessStatusCode)
                {
                    App.Current.MainPage = new ManufacturerSelectionPage();
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    await DisplayAlert("Conflict", "Previous user delete operation is in progress. Please try again later", "Ok");
                }
                else
                {
                    await DisplayAlert("Error", "Something went wrong. Please try again", "Ok");
                    Console.WriteLine(result.ToString());
                }
            }
        }
    }
}
