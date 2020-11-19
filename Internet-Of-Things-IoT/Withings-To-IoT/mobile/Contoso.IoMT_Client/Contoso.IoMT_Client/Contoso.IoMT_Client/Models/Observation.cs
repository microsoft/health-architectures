// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using System.ComponentModel;
    using System.Globalization;
    using Microcharts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public static class Serialize
    {
        public static string ToJson(this List<Observation> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public partial class Observation
    {
        private bool isSelected;

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

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("category")]
        public List<Category> Category { get; set; }

        [JsonProperty("code")]
        public Code Code { get; set; }

        [JsonProperty("subject")]
        public Subject Subject { get; set; }

        [JsonProperty("encounter")]
        public object Encounter { get; set; }

        [JsonProperty("effectiveDateTime")]
        public DateTimeOffset EffectiveDateTime { get; set; }

        [JsonProperty("issued")]
        public DateTimeOffset Issued { get; set; }

        public string Time
        {
            get { return Issued.TimeOfDay.ToString("t"); }
        }

        public string ObservationStatus
        {
            get { return "Normal"; }
        }

        [JsonProperty("valueQuantity")]
        public ValueQuantity ValueQuantity { get; set; }

        [JsonProperty("device")]
        public object Device { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class Category
    {
        [JsonProperty("coding")]
        public List<Coding> Coding { get; set; }
    }

    public partial class Coding
    {
        [JsonProperty("system")]
        public Uri System { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }
    }

    public partial class Code
    {
        [JsonProperty("coding")]
        public List<Coding> Coding { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class Subject
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("identifier")]
        public object Identifier { get; set; }

        [JsonProperty("display")]
        public object Display { get; set; }
    }

    public partial class ValueQuantity
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("system")]
        public Uri System { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public partial class Observation
    {
        public static List<Observation> FromJson(string json) => JsonConvert.DeserializeObject<List<Observation>>(json, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
            },
        };
    }
}
