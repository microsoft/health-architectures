// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Controls
{
    using Xamarin.Forms;

    public class LineGraphOptions
    {
        public Color BackgroundColor { get; set; }

        public Color BandColor { get; set; }

        public Color LineColor { get; set; }

        public Color MarkerTextColor { get; set; }

        public float XAxisLabelOffset { get; set; }

        public float YAxisLabelOffset { get; set; }

        public float AxisStrokeWidth { get; set; }

        public float LineStrokeWidth { get; set; }

        public float MarkerDefaultRadius { get; set; }

        public float LabelTextSize { get; set; }

        public GraphPadding Padding { get; set; }

        public static LineGraphOptions Default()
        {
            return new LineGraphOptions
            {
                BackgroundColor = Color.White,
                BandColor = Color.FromHex("#CCCCCC"),
                LineColor = Color.FromHex("#64A3F8"),
                MarkerTextColor = Color.Black,
                XAxisLabelOffset = 2f,
                YAxisLabelOffset = 2f,
                LineStrokeWidth = 2f,
                AxisStrokeWidth = 0.5f,
                MarkerDefaultRadius = 3f,
                LabelTextSize = 14f,
                Padding = new GraphPadding { Bottom = 5, Left = 5, Right = 10, Top = 10 },
            };
        }
    }
}
