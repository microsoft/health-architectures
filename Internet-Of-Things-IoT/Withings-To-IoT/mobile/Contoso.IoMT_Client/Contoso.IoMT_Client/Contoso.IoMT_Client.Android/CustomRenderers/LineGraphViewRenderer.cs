// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Contoso.IoMT_Client.Controls;
using Contoso.IoMT_Client.Droid.CustomRenderers;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(LineGraphView), typeof(LineGraphViewRenderer))]

namespace Contoso.IoMT_Client.Droid.CustomRenderers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Android.Content;
    using Android.Graphics;
    using Contoso.IoMT_Client.Models;
    using Xamarin.Forms;
    using Xamarin.Forms.Platform.Android;
    using Color = Android.Graphics.Color;
    using Rect = Android.Graphics.Rect;

    public class LineGraphViewRenderer : BoxRenderer
    {
        private Paint paint = new Paint();

        public LineGraphViewRenderer(Context context)
            : base(context)
        {
        }

        ~LineGraphViewRenderer()
        {
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == LineGraphView.DataProperty.PropertyName
                || e.PropertyName == LineGraphView.OptionsProperty.PropertyName
                || e.PropertyName == VisualElement.WidthProperty.PropertyName
                || e.PropertyName == VisualElement.WidthRequestProperty.PropertyName
                || e.PropertyName == VisualElement.HeightProperty.PropertyName
                || e.PropertyName == VisualElement.HeightRequestProperty.PropertyName)
            {
                this.Invalidate();
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var data = ((LineGraphView)Element).Data;
            var options = ((LineGraphView)Element).Options;

            DrawPlot(canvas, options, data);
        }

        private void DrawPlot(Canvas canvas, LineGraphOptions options, IEnumerable<LineGraphData> items)
        {
            var density = Resources.DisplayMetrics.Density;

            var backgroundColor = options.BackgroundColor.ToAndroid();
            var bandsColor = options.BandColor.ToAndroid();
            var lineColor = options.LineColor.ToAndroid();
            var markerTextColor = options.MarkerTextColor.ToAndroid();
            var xAxisLabelOffset = options.XAxisLabelOffset * density;
            var yAxisLabelOffset = options.YAxisLabelOffset * density;
            var lineStrokeWidth = options.LineStrokeWidth * density;
            var axisStrokeWidth = options.AxisStrokeWidth * density;
            var markerDefaultRadius = options.MarkerDefaultRadius * density;
            var labelTextSize = options.LabelTextSize * density;
            var sectionCount = 4;

            // Draw background rectangle
            paint.Color = backgroundColor;
            canvas.DrawRect(new Rect(0, 0, this.Width, this.Height), paint);

            // Set text size
            paint.TextSize = labelTextSize;
            var max = items.Max(i => i.Value) * 1.05;
            var min = items.Min(i => i.Value) * 0.95;
            var ceilingValue = Math.Ceiling(max / 50.0) * 50.0;

            var plotBoundaries = new Boundaries
            {
                Left = (options.Padding.Left * density) + paint.MeasureText(ceilingValue.ToString()) + yAxisLabelOffset,
                Right = this.Width - (options.Padding.Right * density),
                Top = options.Padding.Top * density,
                Bottom = this.Height - (options.Padding.Bottom * density) - paint.TextSize - xAxisLabelOffset,
            };
            var plotWidth = plotBoundaries.Right - plotBoundaries.Left;
            var plotHeight = plotBoundaries.Bottom - plotBoundaries.Top;

            var verticalSection = new Section
            {
                Width = plotWidth / items.Count(),
                Count = items.Count(),
            };

            var horizontalSection = new Section
            {
                Max = (float)(max - min),
                Count = sectionCount,
                Width = plotHeight / sectionCount,
            };

            // Draw horizontal bands
            paint.Reset();
            paint.Color = bandsColor;
            for (int i = horizontalSection.Count - 1; i >= 0; i--/*= i - 2*/)
            {
                var y = plotBoundaries.Bottom - (horizontalSection.Width * i);

                canvas.DrawLine(
                    startX: plotBoundaries.Left,
                    startY: y,
                    stopX: plotBoundaries.Right,
                    stopY: y,
                    paint: paint);
            }

            // Calculate data coordinates
            var points = new List<Tuple<float, float, string, double, bool>>();
            foreach (var l in items.Select((l, index) => Tuple.Create(l.Label, l.Value - min, index)))
            {
                var x = (verticalSection.Width * (l.Item3 + 0.5f)) + plotBoundaries.Left;
                var y = (float)l.Item2 * plotHeight / horizontalSection.Max;

                float left = plotBoundaries.Left;
                points.Add(
                    Tuple.Create(
                        x,
                        plotBoundaries.Bottom - y,
                        l.Item1,
                        l.Item2,
                        left + (l.Item3 * verticalSection.Width) <= 0 && plotBoundaries.Left + ((l.Item3 + 1) * verticalSection.Width) > 0));
            }

            // Draw X axis labels
            paint.Reset();
            paint.TextAlign = Paint.Align.Center;
            paint.TextSize = labelTextSize;
            foreach (var p in points)
            {
                if (p.Item5)
                {
                    paint.Color = markerTextColor;

                    canvas.DrawText(
                        text: p.Item3,
                        x: p.Item1,
                        y: plotBoundaries.Bottom + paint.TextSize + xAxisLabelOffset,
                        paint: paint);
                }
                else
                {
                    paint.Color = lineColor;

                    canvas.DrawText(
                        text: p.Item3,
                        x: p.Item1,
                        y: plotBoundaries.Bottom + paint.TextSize + xAxisLabelOffset,
                        paint: paint);
                }
            }

            // Draw Y axis labels
            // The 1.5f * density on y is a hack to get the label aligned vertically.
            // It will need adjustements if the font size changes.
            paint.Reset();
            paint.TextAlign = Paint.Align.Right;
            paint.TextSize = labelTextSize;
            paint.Color = lineColor;
            double step = (max - min) / sectionCount;
            int j = 0;
            for (double i = min; i < max; i += step, j++)
            {
                var y = plotBoundaries.Bottom - (horizontalSection.Width * j);

                canvas.DrawText(
                    text: Math.Round(i).ToString(),
                    x: plotBoundaries.Left - yAxisLabelOffset,
                    y: (float)y - ((paint.Ascent() / 2f) + (1.5f * density)),
                    paint: paint);
            }

            // Draw main line
            paint.Reset();
            paint.StrokeWidth = lineStrokeWidth;
            paint.Color = lineColor;
            for (int i = 0; i < points.Count; i++)
            {
                if (i < points.Count - 1)
                {
                    canvas.DrawLine(
                        points[i].Item1,
                        points[i].Item2,
                        points[i + 1].Item1,
                        points[i + 1].Item2,
                        paint);
                }

                paint.SetStyle(Paint.Style.Fill);
                paint.Color = Color.White;
                canvas.DrawCircle(
                    cx: points[i].Item1,
                    cy: points[i].Item2,
                    radius: markerDefaultRadius,
                    paint: paint);
                paint.SetStyle(Paint.Style.Stroke);
                paint.Color = lineColor;
                canvas.DrawCircle(
                    cx: points[i].Item1,
                    cy: points[i].Item2,
                    radius: markerDefaultRadius,
                    paint: paint);
            }
        }
    }
}
