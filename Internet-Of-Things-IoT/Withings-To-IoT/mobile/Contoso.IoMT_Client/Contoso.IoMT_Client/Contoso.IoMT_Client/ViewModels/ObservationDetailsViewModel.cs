// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.ViewModels
{
    using System.Linq;

    public class ObservationDetailsViewModel : BaseViewModel
    {
        private Models.SummaryObservation summaryObservation;
        private Metric primaryMetric;
        private Metric secondaryMetric;

        public Models.SummaryObservation SummaryObservation
        {
            get
            {
                return summaryObservation;
            }

            set
            {
                if (summaryObservation != value)
                {
                    summaryObservation = value;
                    CalculateMetrics();
                    OnPropertyChanged();
                }
            }
        }

        public Metric PrimaryMetric
        {
            get
            {
                return primaryMetric;
            }
        }

        public Metric SecondaryMetric
        {
            get
            {
                return secondaryMetric;
            }

            set
            {
                if (secondaryMetric != value)
                {
                    secondaryMetric = value;
                    OnPropertyChanged();
                }
            }
        }

        private void CalculateMetrics()
        {
            primaryMetric = null;
            secondaryMetric = null;

            if (summaryObservation == null)
            {
                return;
            }

            var lastReading = summaryObservation.Observations.OrderBy(x => x.EffectiveDateTime).Last();
            primaryMetric = new Metric
            {
                Label = $"Current {summaryObservation.Category}",
                Value = lastReading.ValueQuantity.Value.ToString(),
                Unit = lastReading.ValueQuantity.Unit,
                Status = "Healthy",
            };

            switch (summaryObservation.Category.ToLower())
            {
                case "blood pressure":
                    break;
                case "heart rate":
                    break;
                case "body weight":
                    secondaryMetric = new Metric
                    {
                        Label = "Current BMI",
                        Value = "???",
                    };
                    break;
            }
        }

        public class Metric
        {
            public string Label { get; set; }

            public string Value { get; set; }

            public string Unit { get; set; }

            public string Status { get; set; }
        }
    }
}
