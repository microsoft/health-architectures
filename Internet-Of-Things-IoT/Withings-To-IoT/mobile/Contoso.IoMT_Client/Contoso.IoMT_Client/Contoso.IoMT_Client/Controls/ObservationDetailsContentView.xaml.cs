// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Controls
{
    using System.Collections.Generic;
    using Contoso.IoMT_Client.Models;
    using Contoso.IoMT_Client.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObservationDetailsContentView : ContentView
    {
        private ObservationDetailsViewModel viewModel = null;
        private Page parent = null;

        public ObservationDetailsContentView()
        {
            InitializeComponent();
        }

        public void InitializeViewModel(Page parent, ObservationDetailsViewModel viewModel)
        {
            this.parent = parent;
            this.viewModel = viewModel;

            this.BindingContext = this.viewModel;
        }

        private void NewMeasurementButton_Clicked(object sender, System.EventArgs e)
        {
            // TODO: Implement something here, preferrably move to VM using Command pattern
        }
    }
}
