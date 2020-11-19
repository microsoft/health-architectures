// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Contoso.IoMT_Client.Models;
    using Xamarin.Forms;

    public class ModelToImageSourceValueConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            List<Coding> val = (List<Coding>)value;
            string retVal = string.Empty;

            string strModelId = (from c in val
                               where c.System.AbsoluteUri.ToString() == $"{Constants.Manufacturers.Withings.Value}/device/model_id"
                               select c.Code).FirstOrDefault();
            int model_id = string.IsNullOrEmpty(strModelId) ? -99999 : int.Parse(strModelId);

            switch (model_id)
            {
                case 45:
                    retVal = "HMJR2_AV3.png";
                    break;
                case 7:
                    retVal = "HMJM2_AV3.png";
                    break;
                case 123:
                    retVal = "HMJL2.png";
                    break;

                default:
                    break;
            }

            return retVal;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
