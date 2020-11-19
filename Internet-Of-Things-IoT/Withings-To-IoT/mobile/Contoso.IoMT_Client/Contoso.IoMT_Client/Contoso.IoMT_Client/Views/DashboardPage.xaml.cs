// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Contoso.IoMT_Client.Controls;
    using Contoso.IoMT_Client.Models;
    using Contoso.IoMT_Client.ViewModels;
    using Newtonsoft.Json;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardPage : ContentPage
    {
        public static readonly BindableProperty ObservationsProperty =
            BindableProperty.Create("Observations", typeof(ObservableCollection<Models.Observation>), typeof(DashboardPage), null);

        public static readonly BindableProperty ConnectedDevicesProperty =
            BindableProperty.Create("ConnectedDevices", typeof(ObservableCollection<object>), typeof(DashboardPage), null);

        public static readonly BindableProperty DisconnectedDevicesProperty =
            BindableProperty.Create("DisconnectedDevices", typeof(ObservableCollection<object>), typeof(DashboardPage), null);

        private Dictionary<string, SummaryObservation> observationCategories = new Dictionary<string, SummaryObservation>();

        public DashboardPage()
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }

            InitializeComponent();

            Observations = new ObservableCollection<Models.Observation>();
        }

        public ObservableCollection<Models.Observation> Observations
        {
            get { return (ObservableCollection<Models.Observation>)GetValue(ObservationsProperty); }
            set { SetValue(ObservationsProperty, value); }
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

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override async void OnAppearing()
        {
            await FetchDevices();

            if (ConnectedDevices.Count > 0)
            {
                await FetchObservations();
            }

            BindingContext = this;

            DataTemplate template = (DataTemplate)this.Resources["withingsDeviceDataTemplate"];
            connectedDevicesWrapLayout.Children.Clear();
            foreach (var connectedItem in ConnectedDevices)
            {
                Controls.ContentPresenter contentPresenter = BuildContentPresenter(template, connectedItem, false, true);
                connectedDevicesWrapLayout.Children.Add(contentPresenter);
            }

            template = (DataTemplate)this.Resources["withingsObservationDataTemplate"];
            observationListWrapLayout.Children.Clear();
            foreach (var observation in observationCategories)
            {
                Controls.ContentPresenter observationPresenter = BuildObservationPresenter(template, observation.Value);
                observationListWrapLayout.Children.Add(observationPresenter);
            }

            ManageDevicesButton devicesButton = new ManageDevicesButton();
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new DeviceManagementPage(ConnectedDevices.ToList(), DisconnectedDevices.ToList()));
            };
            devicesButton.GestureRecognizers.Add(tapGestureRecognizer);
            connectedDevicesWrapLayout.Children.Add(devicesButton);

            base.OnAppearing();
        }

        // TODO: Combine this with code from DeviceManagementPage.cs
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

            // TODO: Change device tapped to go somewhere else?
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new DeviceManagementPage(ConnectedDevices.ToList(), DisconnectedDevices.ToList()));
            };

            contentPresenter.GestureRecognizers.Add(tapGestureRecognizer);
            return contentPresenter;
        }

        private Controls.ContentPresenter BuildObservationPresenter(DataTemplate template, Models.SummaryObservation observation)
        {
            Controls.ContentPresenter presenter = new Controls.ContentPresenter()
            {
                BindingContext = observation,
            };
            presenter.ItemTemplate = template;
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();

            // TODO: Change device tapped to go somewhere else?
            tapGestureRecognizer.Tapped += Observation_Tapped;
            presenter.GestureRecognizers.Add(tapGestureRecognizer);
            return presenter;
        }

        private async void Observation_Tapped(object sender, EventArgs e)
        {
            var contentPresenter = (Controls.ContentPresenter)sender;
            var summaryObservation = (Models.SummaryObservation)contentPresenter.BindingContext;

            if (Xamarin.Forms.Device.Idiom == TargetIdiom.Phone)
            {
                await Navigation.PushAsync(new ObservationDetailsPage(summaryObservation));
            }
            else
            {
                ObservationDetailsViewModel viewModel = new ObservationDetailsViewModel();
                if (summaryObservation != null)
                {
                    viewModel.SummaryObservation = summaryObservation;
                }

                observationDetailsContentView.InitializeViewModel(this, viewModel);
                observationDetailsContentView.IsVisible = true;
            }
        }

        private async Task FetchDevices()
        {
            List<object> connectedDevices = new List<object>();
            List<object> disconnectedDevices = new List<object>();

            Console.WriteLine("FetchDevices");

            if (await Services.WithingsService.GetDevices(connectedDevices, disconnectedDevices) == System.Net.HttpStatusCode.OK)
            {
                ConnectedDevices = new ObservableCollection<object>(connectedDevices);
                DisconnectedDevices = new ObservableCollection<object>(disconnectedDevices);
            }
        }

        private async Task FetchObservations()
        {
            try
            {
                Console.WriteLine("FetchObservations");

                var result = await Services.WithingsService.GetObservationsAsync();

                if (result.IsSuccessStatusCode)
                {
                    var observationsJson = await result.Content.ReadAsStringAsync();

                    var observations = Models.Observation.FromJson(observationsJson);
                    var types = observations.Select(x => x.Code.Text).Distinct();
                    foreach (var observationType in types)
                    {
                        observationCategories[observationType] = new SummaryObservation
                        {
                            Category = observationType,
                        };
                        foreach (var ob in observations.Where(y => y.Code.Text == observationType))
                        {
                            observationCategories[observationType].Observations.Add(ob);
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Failed", "Failed loading observations.", "Ok");
                }
            }
            catch (Exception ex)
            {
                // TODO
                Console.WriteLine(ex.Message);
            }
        }
    }
}
