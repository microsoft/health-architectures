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
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FHIRProxy.preprocessors
{
    class ProfileValidationPreProcess : IProxyPreProcess
    {
        public async Task<ProxyProcessResult> Process(string requestBody, HttpRequest req, ILogger log, ClaimsPrincipal principal, string res, string id, string hist, string vid)
        {
            /*Call Resource and Profile Validation server*/
            /* Pass Validation profile names in ms-fp-profile query parameter or specify empty ms-fp-profile validation parameter for schema validation only*/
            if (string.IsNullOrEmpty(requestBody) || req.Method.Equals("GET") || req.Method.Equals("DELETE")) return new ProxyProcessResult(true, "", requestBody, null);
            if (!req.Query.ContainsKey("ms-fp-profile")) return new ProxyProcessResult(true, "", requestBody, null);
            string url = Environment.GetEnvironmentVariable("FHIRVALIDATION_URL");
            if (string.IsNullOrEmpty(url)) {
                log.LogWarning("The validation URL is not configured....Validatioh will not be run");
                return new ProxyProcessResult(true, "", requestBody, null);
            }
           
                using (WebClient client = new WebClient())
            {
                if (req.Query.ContainsKey("ms-fp-profile"))
                {
                    foreach (string v in req.Query["ms-fp-profile"])
                    {
                        if(!string.IsNullOrEmpty(v)) client.QueryString.Add("profile", v);
                    }
                    
                }
                string result = "";
                try
                {
                    byte[] response =
                     client.UploadData(url , System.Text.Encoding.UTF8.GetBytes(requestBody));
                    result = System.Text.Encoding.UTF8.GetString(response);
                }
                catch (WebException we)
                {
                    HttpWebResponse webresponse = (System.Net.HttpWebResponse)we.Response;
                    FHIRResponse r = new FHIRResponse();
                    r.StatusCode = webresponse.StatusCode;
                    r.Content = Utils.genOOErrResponse("web-exception", we.Message);
                    return new ProxyProcessResult(false, "web-exception", requestBody, r);
                }
                JObject obj = JObject.Parse(result);
                //The validator should return an OperationOutcome resource.  The presence of issues indicates a errors/warnings so we will send it back to the client for 
                //corrective action
                JArray issues = (JArray)obj["issue"];
                if (!issues.IsNullOrEmpty())
                {
                    FHIRResponse resp = new FHIRResponse();
                    resp.Content = result;
                    resp.StatusCode = HttpStatusCode.BadRequest;
                    return new ProxyProcessResult(false, "Validation Error", requestBody, resp);
                }
               
            }
            return new ProxyProcessResult(true,"",requestBody,null);
        }
    }
}
