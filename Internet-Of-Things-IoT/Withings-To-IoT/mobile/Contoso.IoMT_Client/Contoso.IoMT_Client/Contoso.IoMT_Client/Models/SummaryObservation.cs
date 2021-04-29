// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microcharts;
    using Newtonsoft.Json;
    using SkiaSharp;

    public partial class SummaryObservation
    {
        private bool isSelected;

        private Dictionary<string, Chart> charts = new Dictionary<string, Chart>();

        public SummaryObservation()
        {
            Observations = new List<Observation>();
        }

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

        public string Category { get; set; }

        public Chart Chart
        {
            get
            {
                if (charts.ContainsKey(Category))
                {
                    return charts[Category];
                }
                else
                {
                    switch (Category)
                    {
                        case "Blood Pressure":
                            charts[Category] = DrawBPChart();
                            break;
                        case "Body Weight":
                            charts[Category] = DrawWeightChart();
                            break;
                        default:
                            return null;
                    }

                    return charts[Category];
                }
            }
        }

        public Chart DetailedChart
        {
            get
            {
                var key = "Detailed" + Category;
                if (charts.ContainsKey(key))
                {
                    return charts[key];
                }
                else
                {
                    switch (Category)
                    {
                        case "Blood Pressure":
                            charts[key] = DrawBPChart(10, true);
                            break;
                        case "Body Weight":
                            charts[key] = DrawWeightChart(10, true);
                            break;
                        default:
                            return null;
                    }

                    return charts[key];
                }
            }
        }

        public DateTime LastReading
        {
            get
            {
                var reading = Observations.OrderBy(x => x.EffectiveDateTime).Last().EffectiveDateTime;
                return reading.DateTime;
            }
        }

        public string LastReadingTime
        {
            get
            {
                return LastReading.ToShortTimeString();
            }
        }

        public string DisplayText
        {
            get
            {
                switch (Category)
                {
                    case "Blood Pressure":
                        var systolic = Observations.Where(y => y.ValueQuantity.Code == "systolic blood pressure").OrderBy(x => x.EffectiveDateTime).Last().ValueQuantity.Value;
                        var diastolic = Observations.Where(y => y.ValueQuantity.Code == "diastolic blood pressure").OrderBy(x => x.EffectiveDateTime).Last().ValueQuantity.Value;
                        return string.Format("{0}/{1}", systolic, diastolic);
                    case "Heart rate":
                        var heartrate = Observations.OrderBy(x => x.EffectiveDateTime).Last().ValueQuantity.Value;
                        return heartrate.ToString();
                    case "Body Weight":
                        var weight = Observations.OrderBy(x => x.EffectiveDateTime).Last().ValueQuantity.Value;
                        return weight.ToString();
                    default:
                        return string.Empty;
                }
            }
        }

        public string Units
        {
            get
            {
                switch (Category)
                {
                    case "Blood Pressure":
                        return "mmHg";
                    case "Body Weight":
                        return "kg";
                    case "Heart rate":
                        return "bpm";
                    default:
                        return string.Empty;
                }
            }
        }

        public string ObservationStatus
        {
            get
            {
                switch (Category)
                {
                    case "Blood Pressure":
                        return "Normal";
                    case "Body Weight":
                        return "Normal";
                    case "Heart rate":
                        return "Normal";
                    default:
                        return string.Empty;
                }
            }
        }

        public List<Observation> Observations { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Chart DrawBPChart(int maxRecords = 5, bool includeGrid = false)
        {
            var bpData = from systolic in Observations
                         join diastolic in Observations on systolic.EffectiveDateTime equals diastolic.EffectiveDateTime
                         where systolic.ValueQuantity.Code.ToLower() == "systolic blood pressure" &&
                             diastolic.ValueQuantity.Code.ToLower() == "diastolic blood pressure"
                         orderby diastolic.EffectiveDateTime descending
                         select new
                         {
                             time = systolic.EffectiveDateTime,
                             systolicValue = systolic.ValueQuantity.Value,
                             diastolicValue = diastolic.ValueQuantity.Value,
                         };

            var entries = new List<ChartEntry>();
            foreach (var point in bpData.Take(maxRecords))
            {
                entries.Add(new ChartEntry(point.systolicValue)
                {
                    // TODO: Color-code points based on healthy range
                    Color = point.systolicValue < 120 ? SKColor.Parse("#F2A1A5") : SKColor.Parse("#E74E54"),
                });

                entries.Add(new ChartEntry(point.diastolicValue)
                {
                    Color = point.diastolicValue < 80 ? SKColor.Parse("#F2A1A5") : SKColor.Parse("#E74E54"),
                });
            }

            // TODO: Need high-lo chart
            var chart = new PointChart
            {
                Entries = entries,
            };

            return chart;
        }

        private Chart DrawWeightChart(int maxRecords = 5, bool includeGrid = false)
        {
            var weightData = Observations
                .OrderByDescending(y => y.EffectiveDateTime)
                .Take(maxRecords);

            var entries = new List<ChartEntry>();
            foreach (var point in weightData)
            {
                entries.Add(new ChartEntry(point.ValueQuantity.Value)
                {
                    Color = SKColor.Parse("#64A3F8"),
                });
            }

            var chart = new LineChart
            {
                Entries = entries,
                EnableYFadeOutGradient = true,
            };

            return chart;
        }
    }

    public partial class SummaryObservation
    {
        public static List<SummaryObservation> FromJson(string json) => JsonConvert.DeserializeObject<List<SummaryObservation>>(json, Converter.Settings);
    }
}
