// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class ObservationStatusToColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value?.ToString();
            switch (val?.ToLower())
            {
                case "normal":
                case "healthy":
                    return Color.FromHex("65BF81");
                case "low hr":
                case "low":
                    return Color.FromHex("E74E54");
                case "high hr":
                case "high":
                    return Color.FromHex("D9A85B");
            }

            return Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}