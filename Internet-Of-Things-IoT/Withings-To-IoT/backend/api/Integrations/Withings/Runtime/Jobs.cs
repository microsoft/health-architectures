// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Withings.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using H3.Core.Runtime;
    using H3.Integrations.Withings.Domain;
    using H3.Integrations.Withings.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements all the background jobs required to integrate with the Withings IoMT API:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="WithingsRunDeviceIngestion"/> implements the background process to import all historical data
    ///     from one or more Withings devices into the system.
    ///     <a href="https://developer.withings.com/oauth2/#operation/measure-getmeas">See documentation.</a>
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="WithingsRunDeviceDeletion"/> implements the background process to purge all data associated
    ///     with one or more Withings devices from the system.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="WithingsDeviceNotificationMessage"/>, <see cref="WithingsDeviceNotificationWorkflow"/> and <see cref="WithingsRunDeviceNotificationIngestion"/>
    ///     implement the background process to ingest newly arriving data into the system when <see cref="Apis.WithingsCallback"/> is invoked.
    ///     Given that the callback endpoint must return within a short timeframe, the ingestion process uses two separate steps: first the notification
    ///     that new data is available is enqueued immediately when the HTTP request is handled without further processing. Then, a background job
    ///     picks up the message, performs validation and to avoid concurrency issues waits for the lock on the user's account to become available. Finally, the actual data
    ///     ingestion logic is run.
    ///     <a href="https://developer.withings.com/oauth2/#section/DATA-API/Notifications">See documentation.</a>
    ///   </description></item>
    /// </list>
    /// </summary>
    public class Jobs
    {
        public const string NotificationQueueName = "withingsnotification";

        private readonly ILogger<Jobs> log;
        private readonly IExceptionFilter exceptionFilter;
        private readonly IFhirClient fhirClient;
        private readonly IJson json;
        private readonly IWithingsClient withingsClient;
        private readonly IConsentStore consentStore;
        private readonly INotification notification;
        private readonly IGuidFactory guidFactory;
        private readonly IWithingsToFhirConverter converter;

        public Jobs(
            ILoggerFactory log,
            IExceptionFilter exceptionFilter,
            IFhirClient fhirClient,
            IJson json,
            IWithingsClient withingsClient,
            IConsentStore consentStore,
            INotification notification,
            IGuidFactory guidFactory,
            IWithingsToFhirConverter converter)
        {
            this.log = log.CreateLogger<Jobs>();
            this.exceptionFilter = exceptionFilter;
            this.fhirClient = fhirClient;
            this.json = json;
            this.withingsClient = withingsClient;
            this.consentStore = consentStore;
            this.notification = notification;
            this.guidFactory = guidFactory;
            this.converter = converter;
        }

        [FunctionName(nameof(WithingsDeviceNotificationMessage))]
        public async Task WithingsDeviceNotificationMessage(
            [ServiceBusTrigger(NotificationQueueName, Connection = "SB_CONNECTION_STRING")] string queueItem,
            [DurableClient] IDurableOrchestrationClient jobClient,
            CancellationToken cancellationToken)
        {
            var message = json.Load<CallbackReceivedMessage>(queueItem);

            var consent = await consentStore.FetchConsentByExternalId(converter.System, message.WithingsUserId.ToString(), cancellationToken);

            if (consent == null)
            {
                log.LogWarning("Skipping Withings notification for unknown user: withingsUserId={withingsUserId}", message.WithingsUserId);
                return;
            }

            await jobClient.RunSingleton(
                consent.UserId,
                workflow: nameof(WithingsDeviceNotificationWorkflow),
                jobArguments: new StartNotificationIngestionMessage
                {
                    UserId = consent.UserId,
                    StartDateEpoch = message.StartDateEpoch,
                    EndDateEpoch = message.EndDateEpoch,
                },
                log,
                guidFactory,
                blocking: true);
        }

        [FunctionName(nameof(WithingsDeviceNotificationWorkflow))]
        public async Task WithingsDeviceNotificationWorkflow(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext)
        {
            var input = orchestrationContext.GetInput<StartNotificationIngestionMessage>();

            await orchestrationContext.CallActivityAsync(nameof(WithingsRunDeviceNotificationIngestion), input);
        }

        [FunctionName(nameof(WithingsRunDeviceNotificationIngestion))]
        public Task WithingsRunDeviceNotificationIngestion(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartNotificationIngestionMessage>();
                var userId = message.UserId;
                var startDate = DateTimeOffset.FromUnixTimeSeconds(message.StartDateEpoch);
                var endDate = DateTimeOffset.FromUnixTimeSeconds(message.EndDateEpoch);

                var consent = await consentStore.FetchConsent(userId, cancellationToken);
                consent = consent.Verify(userId);

                var measurementsTask = withingsClient.FetchMeasurements(userId, startDate, endDate, cancellationToken);
                var devicesTask = withingsClient.FetchDevices(userId, cancellationToken);

                var measurements = await measurementsTask;
                var devices = await devicesTask;

                var fhirDevices = converter.Convert(devices, consent.Devices.Select(d => d.Value).ToArray()).ToArray();
                var fhirObservations = converter.Convert(consent.FhirId, measurements, fhirDevices).ToArray();

                await fhirClient.CreateObservations(userId, fhirObservations, cancellationToken);

                await SendNotifications(
                    consent,
                    messages: fhirObservations
                        .Select(observation => observation.Device)
                        .Distinct()
                        .Select(device => $"New data is available for {device.Display}"),
                    silent: true,
                    cancellationToken);
            });
        }

        [FunctionName(nameof(WithingsRunDeviceDeletion))]
        public Task WithingsRunDeviceDeletion(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartDeviceDeletionMessage>();
                var userId = message.UserId;
                var withingsDeviceIds = message.WithingsDeviceIds;

                var consent = await consentStore.FetchConsent(userId, cancellationToken);
                consent = consent.Verify(userId);

                var devices = await withingsClient.FetchDevices(userId, cancellationToken);
                var fhirDevices = converter.Convert(devices, withingsDeviceIds).ToArray();

                await fhirClient.DeleteObservations(userId, consent.FhirId, converter.ShouldDelete(withingsDeviceIds), cancellationToken);

                await SendNotifications(
                    consent,
                    messages: fhirDevices.Select(device => $"Data is now removed for {device.Display}"),
                    silent: false,
                    cancellationToken);
            });
        }

        [FunctionName(nameof(WithingsRunDeviceIngestion))]
        public Task WithingsRunDeviceIngestion(
            [ActivityTrigger] IDurableActivityContext context,
            CancellationToken cancellationToken)
        {
            return exceptionFilter.FilterExceptions(async () =>
            {
                var message = context.GetInput<StartDeviceIngestionMessage>();
                var userId = message.UserId;
                var withingsDeviceIds = message.WithingsDeviceIds;

                var consent = await consentStore.FetchConsent(userId, cancellationToken);
                consent = consent.Verify(userId);

                var measurementsTask = withingsClient.FetchMeasurements(userId, startDate: null, endDate: null, cancellationToken);
                var devicesTask = withingsClient.FetchDevices(userId, cancellationToken);

                var measurements = await measurementsTask;
                var devices = await devicesTask;

                var fhirDevices = converter.Convert(devices, withingsDeviceIds).ToArray();
                var fhirObservations = converter.Convert(consent.FhirId, measurements, fhirDevices).ToArray();

                await fhirClient.CreateObservations(userId, fhirObservations, cancellationToken);

                await SendNotifications(
                    consent,
                    messages: fhirDevices.Select(device => $"Data is now available for {device.Display}"),
                    silent: false,
                    cancellationToken);
            });
        }

        private Task SendNotifications(Consent consent, IEnumerable<string> messages, bool silent, CancellationToken cancellationToken)
        {
            return Task.WhenAll(messages.Select(message => notification.SendNotification(consent.UserId, consent.MobileDevices, message, silent, cancellationToken)));
        }
    }
}
