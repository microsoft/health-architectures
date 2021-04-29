// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using Contoso.IoMT_Client.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObservationDetailsPage : ContentPage
    {
        private Models.SummaryObservation summaryObservation = null;

        public ObservationDetailsPage(Models.SummaryObservation summaryObservation)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }

            this.summaryObservation = summaryObservation;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            ObservationDetailsViewModel viewModel = new ObservationDetailsViewModel();
            if (summaryObservation != null)
            {
                viewModel.SummaryObservation = summaryObservation;
            }

            contentView.InitializeViewModel(this, viewModel);

            #if __ANDROID__
            //contentView.InitializeViewModel(this, viewModel);
            contentView.SetupLineGraph();
            #endif

            base.OnAppearing();
        }
    }
}
