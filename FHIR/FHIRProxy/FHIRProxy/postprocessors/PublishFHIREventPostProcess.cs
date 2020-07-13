/* 
* 2020 Microsoft Corp
* 
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS”
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
* THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
* HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
* OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
* OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace FHIRProxy.postprocessors
{
    /* Proxy Post Process to publish events for CUD events to FHIR Server */
    class PublishFHIREventPostProcess : IProxyPostProcess
    {
        public ProxyProcessResult Process(FHIRResponse response, HttpRequest req, ILogger log, ClaimsPrincipal principal, string res, string id, string hist, string vid)
        {
            EventHubClient eventHubClient = null;
            try
            {
                if (req.Method.Equals("GET") || (int)response.StatusCode > 299) return new ProxyProcessResult(true, "", "", response);

                string ecs = Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION");
                string enm = Environment.GetEnvironmentVariable("EVENTHUB_NAME");
                if (string.IsNullOrEmpty(ecs) || string.IsNullOrEmpty(enm))
                {
                    log.LogWarning($"PublishFHIREventPostProcess: EventHubConnection String or EventHub Name is not specified. Will not publish.");
                    return new ProxyProcessResult(true, "", "", response);
                }
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(ecs)
                {
                    EntityPath = enm
                };
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
                    var fhirresp = JObject.Parse(response.Content.ToString());
                    if (!fhirresp.IsNullOrEmpty() && ((string)fhirresp["resourceType"]).Equals("Bundle") && ((string)fhirresp["type"]).EndsWith("-response"))
                    {
                        JArray entries = (JArray)fhirresp["entry"];
                        if (!entries.IsNullOrEmpty())
                        {
                            foreach (JToken tok in entries)
                            {
                               
                                JObject entryresp = (JObject)tok["response"];
                                var entrystatus = (string)entryresp["status"];
                                if (entrystatus.Equals("200") || entrystatus.Equals("201"))
                                {
                                    var resource = (JObject)tok["resource"];
                                    publishEvent(eventHubClient, req, resource);
                                } else if (entrystatus.Equals("204"))
                                {
                                    //TODO Handle Deletes in Bundle
                                }
                            }
                        
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.NoContent || fhirresp.IsNullOrEmpty())
                    {
                        JObject stub = new JObject();
                        stub["resourceType"] = res;
                        stub["id"] = id;
                        publishEvent(eventHubClient, req, stub);
                    } else
                    {
                        publishEvent(eventHubClient, req, fhirresp);
                    }
                
                }
            }

            catch (Exception exception)
            {
              log.LogError(exception,$"PublishFHIREventPostProcess Exception: {exception.Message}");
               
            }
            finally
            {
                try
                {
                    if (eventHubClient != null) eventHubClient.Close();
                }
                catch (Exception e) { }

            }
            return new ProxyProcessResult(true, "", "", response);

        }
        private void publishEvent(EventHubClient eventHubClient,HttpRequest req,JObject resource)
        {
            string msg = "{\"action\":\"" + req.Method + "\",\"resourcetype\":\"" + (string)resource["resourceType"] + "\",\"id\":\"" + (string)resource["id"] + "\"}";
            EventData dt = new EventData(Encoding.UTF8.GetBytes(msg));
            eventHubClient.SendAsync(dt).GetAwaiter().GetResult();
        }
    }
}
