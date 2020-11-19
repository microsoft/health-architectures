// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Controls
{
    using System.Collections.Generic;
    using Contoso.IoMT_Client.Models;
    using Xamarin.Forms;

    public class LineGraphView : BoxView
    {
        public static readonly BindableProperty DataProperty =
              BindableProperty.Create(
                  propertyName: "Data",
                  returnType: typeof(IEnumerable<LineGraphData>),
                  declaringType: typeof(LineGraphView),
                  defaultValue: new List<LineGraphData>());

        public static readonly BindableProperty OptionsProperty =
              BindableProperty.Create(
                  propertyName: "Options",
                  returnType: typeof(LineGraphOptions),
                  declaringType: typeof(LineGraphView),
                  defaultValue: LineGraphOptions.Default());

        public IEnumerable<LineGraphData> Data
        {
            get { return (IEnumerable<LineGraphData>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public LineGraphOptions Options
        {
            get { return (LineGraphOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
    }
}
