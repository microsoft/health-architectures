// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class CodeToUnitsColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value.ToString();
            switch (val.ToLower())
            {
                case "blood pressure":
                    return Color.FromHex("F2A1A5");
                case "heart rate":
                    return Color.FromRgba(217, 168, 91, 128);
                case "body weight":
                    return Color.FromRgba(100, 163, 248, 128);
            }

            return Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}