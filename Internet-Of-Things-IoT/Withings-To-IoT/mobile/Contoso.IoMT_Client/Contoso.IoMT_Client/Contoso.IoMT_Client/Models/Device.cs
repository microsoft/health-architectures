// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;

    public class Device : INotifyPropertyChanged
    {
        private bool isSelected;
        private bool showExtraInfo;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool ShowExtraInfo
        {
            get
            {
                return showExtraInfo;
            }

            set
            {
                showExtraInfo = value;
                OnPropertyChanged("ShowExtraInfo");
            }
        }

        public bool IsConnected { get; set; }

        [JsonProperty("identifier")]
        public Identifier Identifier { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Identifier
    {
        [JsonProperty("system")]
        public string System { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public Type Type { get; set; }
    }

    public class Type
    {
        [JsonProperty("coding")]
        public List<Coding> Coding { get; set; }
    }
}
