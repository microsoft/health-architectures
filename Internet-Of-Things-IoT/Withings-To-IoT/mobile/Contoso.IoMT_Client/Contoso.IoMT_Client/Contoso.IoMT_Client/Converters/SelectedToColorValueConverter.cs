// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class SelectedToColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = bool.Parse(value.ToString());
            Color retVal = val ? (Color)App.Current.Resources["selectedItemBorderColor"] : Color.LightGray;

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
