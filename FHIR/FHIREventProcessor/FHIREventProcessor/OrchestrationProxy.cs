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
using Microsoft.Azure.Services.AppAuthentication;
using FHIRProxy;

namespace FHIREventProcessor
{
    public static class OrchestrationProxy
    {
        /*A FHIR server proxy providing Function Key Based Authentication for orchestration access to a secure FHIR Server */
        private static string _bearerToken = null;
        private static object _lock = new object();
        [FunctionName("OrchestrationProxy")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "patch", Route = "fhir/{res}/{id?}")] HttpRequest req,
                         ILogger log, ClaimsPrincipal principal, string res, string id)
        {
            
            /* Load the request contents */
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
                        _bearerToken = FHIRClient.GetOAUTH2BearerToken(System.Environment.GetEnvironmentVariable("FS_RESOURCE"), System.Environment.GetEnvironmentVariable("FS_TENANT_NAME"),
                                                                  System.Environment.GetEnvironmentVariable("FS_CLIENT_ID"), System.Environment.GetEnvironmentVariable("FS_SECRET")).GetAwaiter().GetResult();
                    }
                }
            }
            List<HeaderParm> auditheaders = new List<HeaderParm>();
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-PROXY", "FHIREventProcessor"));
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

            FHIRResponse fhirresp = null;
            /* Proxy the call to the FHIR Server */
            JObject result = null;
            if (req.Method.Equals("GET"))
            {
                var qs = req.QueryString.HasValue ? req.QueryString.Value : null;
                fhirresp = fhirClient.LoadResource(res + (id == null ? "" : "/" + id), qs, false, customandrestheaders.ToArray());
            }
            else
            {
                fhirresp = fhirClient.SaveResource(res,requestBody, req.Method, customandrestheaders.ToArray());
            }
            /* Fix location header to proxy address */
            if (fhirresp.Headers.ContainsKey("Location"))
            {
                fhirresp.Headers["Location"].Value = fhirresp.Headers["Location"].Value.Replace(Environment.GetEnvironmentVariable("FS_URL"), req.Scheme + "://" + req.Host.Value + req.Path.Value.Substring(0, req.Path.Value.IndexOf(res) - 1));
            }
            var str = fhirresp.Content == null ? "" : (string)fhirresp.Content;
            /* Fix server locations to proxy address */
            str = str.Replace(Environment.GetEnvironmentVariable("FS_URL"), req.Scheme + "://" + req.Host.Value + req.Path.Value.Substring(0, req.Path.Value.IndexOf(res) - 1));
            result = JObject.Parse(str);
            /* Add Headers from FHIR Server Response */
            foreach (string key in fhirresp.Headers.Keys)
            {
                req.HttpContext.Response.Headers.Remove(key);
                req.HttpContext.Response.Headers.Add(key, fhirresp.Headers[key].Value);
            }
            var jr = new JsonResult(result);
            jr.StatusCode = (int)fhirresp.StatusCode;
            return jr;

        }

       
    }
}
