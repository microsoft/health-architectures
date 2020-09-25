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

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.ComponentModel.Design;
using Microsoft.Azure.EventHubs;
using System.Text;
using Microsoft.Azure.Services.AppAuthentication;
using FHIRProxy;

namespace FHIREventProcessor
{
    public static class ConversionImport
    {
        private static string _bearerToken = null;
        private static object _lock = new object();
        [FunctionName("Import")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "import/{resourceType?}")] HttpRequest req, string resourceType,
            [EventHub("%EventHubName%", Connection = "EventHubConnection")]IAsyncCollector<EventData> outputEvents,
                         ILogger log)
        {
            log.LogInformation("FHIR Proxy - Import Function Invoked");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            /* Get/update/check current bearer token to authenticate the proxy to the FHIR Server
             * The following parameters must be defined in environment variables:
             * To use Manged Service Identity or Service Client:
             * FS_URL = the fully qualified URL to the FHIR Server
             * FS_RESOURCE = the audience or resource for the FHIR Server for Azure API for FHIR should be https://azurehealthcareapis.com
             * To use a Service Client Principal the following must also be specified:
             * FS_TENANT_NAME = the GUID or UPN of the AAD tenant that is hosting FHIR Server Authentication
             * FS_CLIENT_ID = the client or app id of the private client authorized to access the FHIR Server
             * FS_SECRET = the client secret to pass to FHIR Server Authentication
             */
            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("FS_RESOURCE")) && FHIRClient.isTokenExpired(_bearerToken))
            {
                lock (_lock)
                {
                    if (FHIRClient.isTokenExpired(_bearerToken))
                    {
                        log.LogInformation($"Obtaining new OAUTH2 Bearer Token for access to FHIR Server");
                        _bearerToken = FHIRClient.GetOAUTH2BearerToken(System.Environment.GetEnvironmentVariable("FS_RESOURCE"),System.Environment.GetEnvironmentVariable("FS_TENANT_NAME"),
                                                                  System.Environment.GetEnvironmentVariable("FS_CLIENT_ID"), System.Environment.GetEnvironmentVariable("FS_SECRET")).GetAwaiter().GetResult();
                    }
                }
            }
            /* 
             * Create User Custom Headers these headers are passed to the FHIR Server to communicate credentials of the authorized user for each proxy call
             * this is ensures accruate audit trails for FHIR server access. Note: This headers are honored by the Azure API for FHIR Server
             */

            List<HeaderParm> auditheaders = new List<HeaderParm>();
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-PROXY", "FHIREventProcessor-Import"));
            /* Preserve FHIR Specific change control headers and include in the proxy call */
            List<HeaderParm> customandrestheaders = new List<HeaderParm>();
            foreach (string key in req.Headers.Keys)
            {
                string s = key.ToLower();
                if (s.Equals("etag")) customandrestheaders.Add(new HeaderParm(key, req.Headers[key].First()));
                else if (s.StartsWith("if-")) customandrestheaders.Add(new HeaderParm(key, req.Headers[key].First()));
            }
            /* Add User Audit Headers */
            customandrestheaders.AddRange(auditheaders);
            /* Get a FHIR Client instance to talk to the FHIR Server */
            log.LogInformation($"Instanciating FHIR Client Proxy");
            FHIRClient fhirClient = new FHIRClient(System.Environment.GetEnvironmentVariable("FS_URL"), _bearerToken);
            //Check for transaction bundle
            requestBody = transformTransactionBundles(requestBody, fhirClient, log);
            var result = fhirClient.SaveResource(resourceType, requestBody, "POST", customandrestheaders.ToArray());
            if (result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Created)
            {
                var fhirresp = JObject.Parse((string)result.Content);
                if(fhirresp != null && ((string)fhirresp["resourceType"]).Equals("Bundle") && ((string)fhirresp["type"]).EndsWith("-response"))
                {
                    JArray entries = (JArray)fhirresp["entry"];
                    foreach (JToken tok in entries)
                    {
                        JObject entryresp = (JObject)tok["response"];
                        var entrystatus = (string)entryresp["status"];
                        if (entrystatus.Equals("200") || entrystatus.Equals("201"))
                        {
                            var res = (JObject)tok["resource"];
                            string msg = "{\"effectedresource\":\"" + (string)res["resourceType"] + "\",\"id\":\"" + (string)res["id"] + "\"}";
                            log.LogInformation("Adding Event: " + msg);
                            EventData dt = new EventData(Encoding.UTF8.GetBytes(msg));
                            await outputEvents.AddAsync(dt);
                        }
                    }
                } else
                {
                    string msg = "{\"effectedresource\":\"" + (string)fhirresp["resourceType"] + "\",\"id\":\"" + (string)fhirresp["id"] + "\"}";
                    log.LogInformation("Adding Event: " + msg);
                    EventData dt = new EventData(Encoding.UTF8.GetBytes(msg));
                    await outputEvents.AddAsync(dt);
                }

                return new JsonResult(fhirresp);



            }
            string retval = (string)result.Content;
            if (string.IsNullOrEmpty(retval)) retval = "{\"error\":\"Unexpected Result\"}";
            var jr = new JsonResult(JObject.Parse(retval));
            jr.StatusCode = (int)result.StatusCode;
            return jr;

        

        }
        private static string transformTransactionBundles(string requestBody,FHIRClient fsclient, ILogger log)
        {
            if (string.IsNullOrEmpty(requestBody)) return requestBody;
            JObject result = JObject.Parse(requestBody);
            if (result == null || result["resourceType"] == null || result["type"] == null) return requestBody;
            string rtt = (string)result["resourceType"];
            string bt = (string)result["type"];
            if (rtt.Equals("Bundle") && bt.Equals("transaction"))
            {
                log.LogInformation($"TransformBundleProcess: looks like a valid transaction bundle");
                JArray entries = (JArray)result["entry"];
                if (entries==null) return requestBody;
                log.LogInformation($"TransformBundleProcess: Phase 1 searching for existing entries on FHIR Server...");
                foreach (JToken tok in entries)
                {
                    if (tok !=null && tok["request"]["ifNoneExist"] != null)
                    {
                        string resource = (string)tok["request"]["url"];
                        string query = (string)tok["request"]["ifNoneExist"];
                        log.LogInformation($"TransformBundleProcess:Loading Resource {resource} with query {query}");
                        var r = fsclient.LoadResource(resource, query);
                        if (r.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var rs = (JObject)r.Content;
                            if (rs != null && ((string)rs["resourceType"]).Equals("Bundle") && rs["entry"] != null)
                            {
                                JArray respentries = (JArray)rs["entry"];
                                string existingid = "urn:uuid:" + (string)respentries[0]["resource"]["id"];
                                string furl = (string)tok["fullUrl"];
                                if (!string.IsNullOrEmpty(furl)) requestBody.Replace(furl, existingid);
                            }
                        }
                    }
                }
                //reparse JSON with replacement of existing ids prepare to convert to Batch bundle with PUT to maintain relationships
                Dictionary<string, string> convert = new Dictionary<string, string>();
                result = JObject.Parse(requestBody);
                result["type"] = "batch";
                entries = (JArray)result["entry"];
                foreach (JToken tok in entries)
                {
                    string urn = (string)tok["fullUrl"];
                    if (!string.IsNullOrEmpty(urn) && urn.StartsWith("urn:uuid:") && tok["resource"] != null)
                    {
                        string rt = (string)tok["resource"]["resourceType"];
                        string rid = urn.Replace("urn:uuid:", "");
                        tok["resource"]["id"] = rid;
                        convert.Add(rid, rt);
                        tok["request"]["method"] = "PUT";
                        tok["request"]["url"] = $"{rt}?_id={rid}";
                    }

                }
                log.LogInformation($"TransformBundleProcess: Phase 2 Localizing {convert.Count} resource entries...");
                string str = result.ToString();
                foreach (string id1 in convert.Keys)
                {
                    string r1 = convert[id1] + "/" + id1;
                    string f = "urn:uuid:" + id1;
                    str = str.Replace(f, r1);
                }
                return str;
            }
            return requestBody;
        }
       
    }
}
