// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Droid
{
    using System;
    using System.Linq;
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Support.V4.App;
    using Android.Util;
    using Firebase.Messaging;
    using WindowsAzure.Messaging;
    using WindowsAzure.Messaging.NotificationHubs;

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            SendRegistrationToServer(token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            string messageBody = string.Empty;

            if (message.GetNotification() != null)
            {
                messageBody = message.GetNotification().Body;
            }

            // NOTE: test messages sent via the Azure portal will be received here
            else
            {
                messageBody = message.Data.Values.First();
            }

            // convert the incoming message to a local notification
            SendLocalNotification(messageBody);

            // send the incoming message directly to the MainPage
            SendMessageToMainApp(messageBody);
        }

        private void SendRegistrationToServer(string token)
        {
            try
            {
                App.DeviceId = Android.Provider.Settings.Secure.GetString(
                    Application.Context.ContentResolver,
                    Android.Provider.Settings.Secure.AndroidId);
                string[] tokens = Constants.SubscriptionTags.Append($"deviceid:{App.DeviceId}").ToArray();

                WindowsAzure.Messaging.NotificationHub hub = new WindowsAzure.Messaging.NotificationHub(Constants.NotificationHubName, Constants.ListenConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                Registration registration = hub.Register(token, tokens);

                // subscribe to the SubscriptionTags list with a simple template.
                string pnsHandle = registration.PNSHandle;
                TemplateRegistration templateReg = hub.RegisterTemplate(pnsHandle, "defaultTemplate", Constants.FCMTemplateBody, tokens);
            }
            catch (Exception e)
            {
                Log.Error(Constants.DebugTag, $"Error registering device: {e.Message}");
            }
        }

        private void SendLocalNotification(string body)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);

            var requestCode = new Random().Next();
            var pendingIntent = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, Constants.NotificationChannelName)
                .SetContentTitle("Contoso IoMT Message")
                .SetSmallIcon(Resource.Mipmap.launcher_foreground)
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                notificationBuilder.SetChannelId(Constants.NotificationChannelName);
            }

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        private void SendMessageToMainApp(string body)
        {
            App.AddMessage(body);
        }
    }
}
