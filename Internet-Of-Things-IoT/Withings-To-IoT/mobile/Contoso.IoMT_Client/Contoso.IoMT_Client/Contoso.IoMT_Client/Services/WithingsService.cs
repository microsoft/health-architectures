// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Services
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AppCenter.Analytics;
    using Newtonsoft.Json.Linq;
    using Xamarin.Forms;

    public class WithingsService
    {
        private static HttpClient httpClient;

        static WithingsService()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {App.B2cAccessToken}");
        }

        public static async Task<HttpStatusCode> GetDevices(List<object> connectedWithingDevices, List<object> disconnectedWithingDevices)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            HttpStatusCode retVal = HttpStatusCode.OK;
            var result = await httpClient.GetAsync(Constants.H3BaseEndpoint + Constants.H3UserEndpoint);

            stopWatch.Stop();

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(response);
                JArray connectedDevices = (JArray)jObject["connectedDevices"];
                JArray disconnectedDevices = (JArray)jObject["disconnectedDevices"];
                connectedWithingDevices.AddRange(connectedDevices.ToList());
                disconnectedWithingDevices.AddRange(disconnectedDevices.ToList());

                Analytics.TrackEvent("GetDevices", new Dictionary<string, string>
                {
                    { "Duration", stopWatch.ElapsedMilliseconds.ToString() },
                    { "StatusCode", result.StatusCode.ToString() },
                    { "ConnectedDevices", connectedDevices.Count.ToString() },
                    { "DisconnectedDevices", disconnectedDevices.Count.ToString() },
                });
            }
            else
            {
                retVal = result.StatusCode;

                var response = await result.Content.ReadAsStringAsync();

                Analytics.TrackEvent("GetDevices", new Dictionary<string, string>
                {
                    { "Duration", stopWatch.ElapsedMilliseconds.ToString() },
                    { "StatusCode", result.StatusCode.ToString() },
                    { "Response", response },
                });
            }

            return retVal;
        }

        public static async Task<HttpResponseMessage> UpdateMobileDeviceIdAsync(string mobileDeviceId)
        {
            return await PostMessageAsync($"{{'mobileDevice':{{'deviceId': '{mobileDeviceId}', 'devicePlatform': '{Device.RuntimePlatform}'}}}}");
        }

        public static async Task<HttpResponseMessage> ConnectDeviceAsync(string manufacturer, string deviceId)
        {
            return await PostMessageAsync($"{{'connectedDeviceIds':[{{'system': '{manufacturer}', 'value': '{deviceId}'}}]}}");
        }

        public static async Task<HttpResponseMessage> DisconnectDeviceAsync(string manufacturer, string deviceId)
        {
            return await PostMessageAsync($"{{'disconnectedDeviceIds':[{{'system': '{manufacturer}', 'value': '{deviceId}'}}]}}");
        }

        public static async Task<HttpResponseMessage> DisconnectAccountAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, Constants.H3BaseEndpoint + Constants.H3UserEndpoint);

            var result = await httpClient.SendAsync(request);

            return result;
        }

        public static async Task<HttpResponseMessage> GetObservationsAsync()
        {
            var result = await httpClient.GetAsync(Constants.H3BaseEndpoint + Constants.H3ObservationsEndpoint);

            return result;
        }

        private static async Task<HttpResponseMessage> PostMessageAsync(string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Constants.H3BaseEndpoint + Constants.H3UserEndpoint);

            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await httpClient.SendAsync(request);

            return result;
        }
    }
}
