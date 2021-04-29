// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Xamarin.Forms;

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Page CurrentPage { get; set; }

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RaiseAllProperties()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(name);
        }
    }
}
