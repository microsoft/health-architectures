// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class CodeToValueColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value.ToString();
            switch (val.ToLower())
            {
                case "blood pressure":
                    return Color.FromHex("E74E54");
                case "heart rate":
                    return Color.FromHex("D9A85B");
                case "body weight":
                    return Color.FromHex("64A3F8");
            }

            return Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}