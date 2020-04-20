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
using System.Collections.Specialized;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace FHIRProxy
{
    public class FHIRClient
    {
        private IRestClient _client = null;
        private string auth_tenent = null;
        private string auth_client_id = null;
        private string auth_secret = null;
        private string auth_resource = null;
        public FHIRClient(string baseurl, string bearerToken)
        {
            init(baseurl, bearerToken);
        }
        public FHIRClient(string baseurl, string resource,string tenent = null, string clientid = null, string secret = null)
        {
            auth_tenent = tenent;
            auth_client_id = clientid;
            auth_secret = secret;
            auth_resource = resource;
            string tokenresp = null;
            tokenresp = GetOAUTH2BearerToken(auth_resource, auth_tenent, auth_client_id, auth_secret).GetAwaiter().GetResult();
            init(baseurl, tokenresp);
        }
        public string BearerToken { get; set; }
        private void init(string baseurl, string bearerToken)
        {
            _client = new RestClient(baseurl);
            BearerToken = bearerToken;
        }
        public static bool isTokenExpired(string bearerToken)
        {
            if (bearerToken == null) return true;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(bearerToken) as JwtSecurityToken;
            var tokenExpiryDate = token.ValidTo;

            // If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
            if (tokenExpiryDate == DateTime.MinValue) return true;

            // If the token is in the past then you can't use it
            if (tokenExpiryDate < DateTime.UtcNow) return true;
            return false;

        }
        public static async Task<string> GetOAUTH2BearerToken(string resource, string tenant=null, string clientid=null, string secret=null)
        {
            if (!string.IsNullOrEmpty(resource) && (string.IsNullOrEmpty(tenant) && string.IsNullOrEmpty(clientid) && string.IsNullOrEmpty(secret)))
            {
                //Assume Managed Service Identity with only resource provided.
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var _accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(resource);
                return _accessToken;
            }
            else
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response =
                     client.UploadValues("https://login.microsoftonline.com/" + tenant + "/oauth2/token", new NameValueCollection()
                     {
                        {"grant_type","client_credentials"},
                        {"client_id",clientid},
                        { "client_secret", secret },
                        { "resource", resource }
                     });


                    string result = System.Text.Encoding.UTF8.GetString(response);
                    JObject obj = JObject.Parse(result);
                    return (string)obj["access_token"];
                }
            }
        }
        private void refreshToken()
        {
            if (BearerToken != null && isTokenExpired(BearerToken))
            {
                BearerToken = GetOAUTH2BearerToken(auth_resource, auth_tenent, auth_client_id, auth_secret).GetAwaiter().GetResult();
            }

        }
        private void AddCustomHeadersToRequest(RestRequest req, HeaderParm[] headers)
        {
            if (headers == null || headers.Length == 0) return;
            foreach (HeaderParm p in headers)
            {
                req.AddHeader(p.Name, p.Value);
            }

        }
        public FHIRResponse LoadResource(string resource, string parmstring = null, bool parse = true, HeaderParm[] headers = null)
        {
            refreshToken();
            var request = new RestRequest(resource + (parmstring != null ? (!parmstring.StartsWith("?") ? "?" :"") + parmstring : ""), Method.GET);
            request.AddHeader("Accept", "application/json");
            if (BearerToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + BearerToken);
            }
            AddCustomHeadersToRequest(request, headers);
            IRestResponse response2 = _client.Execute(request);
            return new FHIRResponse(response2, parse);
        }
        public FHIRResponse SaveResource(string reqresource,string content, string method = "PUT", HeaderParm[] headers = null)
        {
            var r = JObject.Parse(content);
            return SaveResource(reqresource, r, method, headers);
        }
        public FHIRResponse SaveResource(string reqresource,JObject r, string method = "PUT", HeaderParm[] headers = null)
        {
            refreshToken();
            Method rm = Method.PUT;
            switch (method)
            {
                case "POST":
                    rm = Method.POST;
                    break;
                case "PUT":
                    rm = Method.PUT;
                    break;
                case "PATCH":
                    rm = Method.PATCH;
                    break;
                case "DELETE":
                    rm = Method.DELETE;
                    break;
                default:
                    throw new Exception($"{method} is not supported for SaveResource");

            }
            string rt = (string)r["resourceType"];
            RestRequest request = null;
            if (string.IsNullOrEmpty(reqresource) && !string.IsNullOrEmpty(rt) && rt.Equals("Bundle"))
            {
                if (rm != Method.POST) throw new Exception("Verb Must be POST for Bundle Processing");
                request = new RestRequest("", rm);
            }
            else
            {
                if (string.IsNullOrEmpty(rt)) throw new Exception("Resource Type not found or is blank in content");
                if ((!rt.Equals(reqresource))) throw new Exception("Resource Request Type must match resource type in content");
                string id = (string)r["id"];
                if (id == null && rm != Method.POST) throw new Exception("Must Specify resource id on modification HTTP Verbs");
                request = new RestRequest(rt + (rm != Method.POST ? "/" + id : ""), rm);
            }
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            if (BearerToken != null)
            {
                request.AddHeader("Authorization", "Bearer " + BearerToken);
            }
            AddCustomHeadersToRequest(request, headers);
            string srv = r.ToString(Formatting.None);
            request.AddParameter("application/json; charset=utf-8", srv, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            IRestResponse response2 = _client.Execute(request);
            return new FHIRResponse(response2);
        }

    }

    public class FHIRResponse
    {
        public FHIRResponse()
        {
            Headers = new Dictionary<string, HeaderParm>();
        }
        public FHIRResponse(IRestResponse resp, bool parse = false) : this()
        {
            string[] filterheaders = null;
            if (Environment.GetEnvironmentVariable("FS_RESPONSE_HEADER_NAME") != null) filterheaders = Environment.GetEnvironmentVariable("FS_RESPONSE_HEADER_NAME").Split(",");
            string content = resp.Content;
            if (parse) this.Content = JObject.Parse(content);
            else this.Content = content;
            foreach (Parameter p in resp.Headers)
            {
                if (filterheaders != null)
                {
                    if (filterheaders.Any(p.Name.Equals))
                    {
                        this.Headers.Add(p.Name, new HeaderParm(p));
                    }
                }
                else
                {
                    this.Headers.Add(p.Name, new HeaderParm(p));
                }

            }
            this.StatusCode = resp.StatusCode;
        }
        public IDictionary<string, HeaderParm> Headers { get; set; }
        public object Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }

    }
    public class HeaderParm
    {
        public HeaderParm()
        {

        }

        public HeaderParm(Parameter p) : this(p.Name, p.ToString())
        {

        }
        public HeaderParm(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}