// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace H3.Integrations.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Newtonsoft.Json;
    using H3.Core.Domain;
    using H3.Core.Models.Api;
    using Microsoft.Azure.NotificationHubs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This class implements the H3 notification pattern using Azure Notification Hubs.
    /// <a href="https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-push-notification-overview">See documentation.</a>
    /// </summary>
    /// <remarks>
    /// The implementation assumes that client devices are responsible for registering the PNS handle
    /// with Azure Notification Hubs and that each user can be addressed via a tag like <c>userid:{the user's id}</c>.
    /// <a href="https://docs.microsoft.com/en-us/azure/notification-hubs/notification-hubs-push-notification-registration-management#registration-management-from-the-device">See documentation.</a>
    /// </remarks>
    public class NotificationHub : INotification
    {
        private readonly ILogger<NotificationHub> log;
        private readonly NotificationHubClient client;
        private readonly IJson json;

        public NotificationHub(ILoggerFactory log, NotificationHubClient client, IJson json)
        {
            this.log = log.CreateLogger<NotificationHub>();
            this.client = client;
            this.json = json;
        }

        public async Task SendNotification(string userId, IReadOnlyCollection<MobileDevice> devices, string message, bool silent, CancellationToken cancellationToken)
        {
            if (devices.Count == 0)
            {
                log.LogWarning("Attempted to send notification for user {userId} without registered device", userId);
                return;
            }

            await Task.WhenAll(devices.GroupBy(device => device.Platform).Select(group =>
            {
                var platform = group.Key;
                var tags = group.Select(device => $"deviceid:{device.Id}");

                return platform switch
                {
                    MobilePlatform.Android => SendNotificationAndroid(userId, message, tags, cancellationToken),
                    MobilePlatform.IOS => SendNotificationIOS(userId, message, tags, silent, cancellationToken),
                    _ => throw new InvalidOperationException(platform.ToString()),
                };
            }));
        }

        private async Task SendNotificationIOS(string userId, string payload, IEnumerable<string> tags, bool silent, CancellationToken cancellationToken)
        {
            var notificationPayload = new IOSNotificationPayload
            {
                Aps = new IOSNotificationContent
                {
                    Alert = payload,
                },
            };

            if (silent)
            {
                notificationPayload.Aps.ContentAvailable = 1;
                notificationPayload.Aps.Sound = string.Empty;
            }

            var notification = new AppleNotification(json.Dump(notificationPayload));

            if (silent)
            {
                notification.Priority = 5;
            }

            log.LogInformation("Sending iOS notification to {userId}", userId);
            var response = await client.SendNotificationAsync(notification, tags, cancellationToken);
            log.LogInformation("Sent iOS notification {trackingId} to {userId}", response.TrackingId, userId);
        }

        private async Task SendNotificationAndroid(string userId, string payload, IEnumerable<string> tags, CancellationToken cancellationToken)
        {
            var notification = json.Dump(new AndroidNotificationPayload
            {
                Data = new AndroidNotificationContent
                {
                    Message = payload,
                },
            });

            log.LogInformation("Sending Android notification to {userId}", userId);
            var response = await client.SendFcmNativeNotificationAsync(notification, tags, cancellationToken);
            log.LogInformation("Sent Android notification {trackingId} to {userId}", response.TrackingId, userId);
        }
    }

    public class AndroidNotificationPayload
    {
        [JsonProperty("data")]
        public AndroidNotificationContent Data { get; set; } = null!;
    }

    public class AndroidNotificationContent
    {
        [JsonProperty("message")]
        public string Message { get; set; } = null!;
    }

    public class IOSNotificationPayload
    {
        [JsonProperty("aps")]
        public IOSNotificationContent Aps { get; set; } = null!;
    }

    public class IOSNotificationContent
    {
        [JsonProperty("alert")]
        public string Alert { get; set; } = null!;

        [JsonProperty("content-available")]
        public int? ContentAvailable { get; set; } = null;

        [JsonProperty("sound")]
        public string? Sound { get; set; } = null;
    }
}
