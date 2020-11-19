// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using H3.Core.Models.Fhir;
    using H3.Integrations.Withings.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class converts between the Withings data model and the FHIR data model, specifically the following schemas:
    /// <list type="bullet">
    ///   <item><description><a href="http://developer.withings.com/oauth2/#operation/measure-getmeas">Measurements</a></description></item>
    ///   <item><description><a href="http://developer.withings.com/oauth2/#operation/userv2-getdevice">Devices</a></description></item>
    /// </list>
    /// </summary>
    public class WithingsToFhirConverter : IWithingsToFhirConverter
    {
        private readonly ILogger log;

        public WithingsToFhirConverter(ILogger log)
        {
            this.log = log;
        }

        public string System => "http://withings.com";

        public IEnumerable<Observation> Convert(string fhirUserId, IEnumerable<Group> measureGroups, IReadOnlyCollection<Ref> withingsDevices)
        {
            foreach (var measureGroup in measureGroups)
            {
                var withingsDevice = withingsDevices.FirstOrDefault(device => device.Identifier.Value == measureGroup.DeviceID);

                if (!IsMeasurement(measureGroup) || withingsDevice == null)
                {
                    continue;
                }

                for (var i = 0; i < measureGroup.Measures.Length; i++)
                {
                    var measure = measureGroup.Measures[i];

                    Observation observation;
                    try
                    {
                        observation = new Observation
                        {
                            ResourceType = "Observation",
                            Id = $"{measureGroup.Id}-{i}",
                            Status = "final",
                            Category = new[]
                            {
                                new Code
                                {
                                    Coding = new[]
                                    {
                                        new Coding
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/observation-category",
                                            Code = "vital-signs",
                                            Display = "vital-signs",
                                        },
                                    },
                                },
                            },
                            Code = ToCode(measure),
                            Subject = new Ref
                            {
                                Reference = $"Patient/{fhirUserId}",
                            },
                            Device = withingsDevice,
                            EffectiveDateTime = ToDateTime(measureGroup.Date),
                            Issued = ToDateTime(measureGroup.Created),
                            ValueQuantity = ToQuantity(measure),
                        };
                    }
                    catch (NotImplementedException ex)
                    {
                        log.LogError("Skipping measure for FHIR user {fhirUserId} in group {id}: {error}", fhirUserId, measureGroup.Id, ex.Message);
                        continue;
                    }

                    yield return observation;
                }
            }
        }

        public IEnumerable<Ref> Convert(IEnumerable<Device> devices, IReadOnlyCollection<string>? withingsDeviceIds)
        {
            foreach (var device in devices)
            {
                if (withingsDeviceIds != null && !withingsDeviceIds.Contains(device.DeviceID))
                {
                    continue;
                }

                yield return new Ref
                {
                    Identifier = new Identifier
                    {
                        System = System,
                        Value = device.DeviceID,
                        Type = new Code
                        {
                            Coding = new Coding[]
                            {
                                new Coding
                                {
                                    System = $"{System}/device/model_id",
                                    Code = device.ModelID.ToString(),
                                },
                                new Coding
                                {
                                    System = $"{System}/device/type",
                                    Code = device.Type,
                                },
                            },
                        },
                    },
                    Display = $"{device.Model} ({device.Type})",
                };
            }
        }

        public Func<Observation, bool> ShouldDelete(IReadOnlyCollection<string> deviceIds)
        {
            return (observation) =>
                observation?.Device?.Identifier?.System == System &&
                deviceIds.Contains(observation?.Device?.Identifier?.Value);
        }

        private static bool IsMeasurement(Group measureGroup)
        {
            return measureGroup.Category == 1;
        }

        private static Code ToCode(Measure measure)
        {
            string text, code;

            switch (measure.Type)
            {
                case 1: text = "Body Weight"; code = "29463-7"; break;
                case 4: text = "Body Height"; code = "8302-2"; break;
                case 9: text = "Blood Pressure"; code = "85354-9"; break;
                case 10: text = "Blood Pressure"; code = "85354-9"; break;
                case 11: text = "Heart rate"; code = "8867-4"; break;
                default: throw new NotImplementedException($"Unknown measure type {measure.Type}");
            }

            return new Code
            {
                Text = text,
                Coding = new[]
                {
                    new Coding
                    {
                        System = "http://loinc.org",
                        Code = code,
                        Display = text,
                    },
                },
            };
        }

        private static string ToDateTime(long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("o");
        }

        private static Quantity ToQuantity(Measure measure)
        {
            string unit, code;

            switch (measure.Type)
            {
                case 1: unit = "kg"; code = "weight"; break;
                case 4: unit = "meter"; code = "height"; break;
                case 9: unit = "mmHg"; code = "diastolic blood pressure"; break;
                case 10: unit = "mmHg"; code = "systolic blood pressure"; break;
                case 11: unit = "bpm"; code = "heart pulse"; break;
                default: throw new NotImplementedException($"Unknown measure type {measure.Type}");
            }

            return new Quantity
            {
                Unit = unit,
                Code = code,
                System = "http://unitsofmeasure.org",
                Value = measure.Value * Math.Pow(10, measure.Unit),
            };
        }
    }
}
