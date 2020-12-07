// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class NullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool p = false;

            if (parameter != null)
            {
                bool.TryParse(parameter.ToString(), out p);
            }

            bool retVal = value == null ? false : true;

            return p ? !retVal : retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
