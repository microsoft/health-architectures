﻿/*
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Services.AppAuthentication;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FHIRProxy
{
    public class FHIRClient
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string auth_client_id;
        private readonly string auth_resource;
        private readonly string auth_secret;
        private readonly string auth_tenent;

        public FHIRClient(string baseurl, string bearerToken)
        {
            init(baseurl, bearerToken);
        }

        public FHIRClient(string baseurl, string resource, string tenent = null, string clientid = null, string secret = null)
        {
            auth_tenent = tenent;
            auth_client_id = clientid;
            auth_secret = secret;
            auth_resource = resource;
            var tokenresp = GetOAUTH2BearerToken(auth_resource, auth_tenent, auth_client_id, auth_secret).GetAwaiter()
                .GetResult();
            init(baseurl, tokenresp);
        }

        private string BearerToken { get; set; }

        private void init(string baseurl, string bearerToken)
        {
            _client.BaseAddress = new Uri(baseurl);
            _client.Timeout = new TimeSpan(0, 0, Utils.GetIntEnvironmentVariable("FS_TIMEOUT_SECS", "30"));
            BearerToken = bearerToken;
        }

        private static HeaderParm[] ToHeaderParmArray(IHeaderDictionary headers)
        {
            var retVal = new List<HeaderParm>();
            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    if (key.StartsWith("X-MS") || key.StartsWith("Prefer") || key.StartsWith("ETag") || key.StartsWith("If-"))

                    {
                        retVal.Add(new HeaderParm(key, headers[key].First()));
                    }
                }
            }

            return retVal.ToArray();
        }

        public static bool isTokenExpired(string bearerToken)
        {
            if (bearerToken == null)
            {
                return true;
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(bearerToken) as JwtSecurityToken;
            var tokenExpiryDate = token.ValidTo;

            // If there is no valid `exp` claim then `ValidTo` returns DateTime.MinValue
            if (tokenExpiryDate == DateTime.MinValue)
            {
                return true;
            }

            // If the token is in the past then you can't use it
            if (tokenExpiryDate < DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        public static async Task<string> GetOAUTH2BearerToken(string resource, string tenant = null, string clientid = null,
            string secret = null)
        {
            if (!string.IsNullOrEmpty(resource) && string.IsNullOrEmpty(tenant) && string.IsNullOrEmpty(clientid) &&
                string.IsNullOrEmpty(secret))
            {
                //Assume Managed Service Identity with only resource provided.
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var _accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(resource);
                return _accessToken;
            }

            using var client = new WebClient();
            var response = client.UploadValues("https://login.microsoftonline.com/" + tenant + "/oauth2/token",
                new NameValueCollection
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", clientid},
                    {"client_secret", secret},
                    {"resource", resource}
                });

            var result = Encoding.UTF8.GetString(response);
            var obj = JObject.Parse(result);
            return (string) obj["access_token"];
        }

        private void refreshToken()
        {
            if (BearerToken != null && isTokenExpired(BearerToken))
            {
                BearerToken = GetOAUTH2BearerToken(auth_resource, auth_tenent, auth_client_id, auth_secret).GetAwaiter()
                    .GetResult();
            }
        }

        private void AddCustomHeadersToRequest(HttpRequestMessage req, HeaderParm[] headers)
        {
            if (headers == null || headers.Length == 0)
            {
                return;
            }

            foreach (var p in headers)
            {
                req.Headers.Add(p.Name, p.Value);
            }
        }

        public async Task<FHIRResponse> LoadResource(string resource, string parmstring = null, bool parse = true,
            IHeaderDictionary headers = null)
        {
            refreshToken();
            var request = new HttpRequestMessage(HttpMethod.Get,
                resource + (parmstring != null ? (!parmstring.StartsWith("?") ? "?" : "") + parmstring : ""));
            request.Headers.Add("Accept", "application/json");
            if (BearerToken != null)
            {
                request.Headers.Add("Authorization", "Bearer " + BearerToken);
            }

            AddCustomHeadersToRequest(request, ToHeaderParmArray(headers));

            var response = await _client.SendAsync(request);

            // Read Response Content (this will usually be JSON content)
            var content = await response.Content.ReadAsStringAsync();

            return new FHIRResponse(content, response.Headers, response.StatusCode, parse);
        }

        public async Task<FHIRResponse> DeleteResource(string resource, IHeaderDictionary headers = null)
        {
            refreshToken();
            var request = new HttpRequestMessage(HttpMethod.Delete, resource);
            request.Headers.Add("Accept", "application/json");
            if (BearerToken != null)
            {
                request.Headers.Add("Authorization", "Bearer " + BearerToken);
            }

            AddCustomHeadersToRequest(request, ToHeaderParmArray(headers));
            var response = await _client.SendAsync(request);

            // Read Response Content (this will usually be JSON content)
            var content = await response.Content.ReadAsStringAsync();

            return new FHIRResponse(content, response.Headers, response.StatusCode);
        }

        public async Task<FHIRResponse> PostCommand(string reqresource, string srccontent, string parmstring,
            IHeaderDictionary headers)
        {
            refreshToken();
            var request = new HttpRequestMessage(HttpMethod.Post,
                reqresource + (parmstring != null ? (!parmstring.StartsWith("?") ? "?" : "") + parmstring : ""));
            if (BearerToken != null)
            {
                request.Headers.Add("Authorization", "Bearer " + BearerToken);
            }

            AddCustomHeadersToRequest(request, ToHeaderParmArray(headers));
            request.Content = new StringContent(srccontent, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _client.SendAsync(request);

            // Read Response Content (this will usually be JSON content)
            var content = await response.Content.ReadAsStringAsync();

            return new FHIRResponse(content, response.Headers, response.StatusCode);
        }

        public async Task<FHIRResponse> SaveResource(string reqresource, string content, string method = "PUT",
            IHeaderDictionary headers = null)
        {
            var r = JObject.Parse(content);
            return await SaveResource(reqresource, r, method, headers);
        }

        public async Task<FHIRResponse> SaveResource(string reqresource, JObject r, string method = "PUT",
            IHeaderDictionary headers = null)
        {
            refreshToken();
            var rm = HttpMethod.Put;
            switch (method)
            {
                case "POST":
                    rm = HttpMethod.Post;
                    break;
                case "PUT":
                    rm = HttpMethod.Put;
                    break;
                case "PATCH":
                    rm = HttpMethod.Patch;
                    break;
                case "DELETE":
                    rm = HttpMethod.Delete;
                    break;
                default:
                    throw new Exception($"{method} is not supported for SaveResource");
            }

            var rt = r.FHIRResourceType();
            HttpRequestMessage request;
            if (string.IsNullOrEmpty(reqresource) && !string.IsNullOrEmpty(rt) && rt.Equals("Bundle"))
            {
                if (rm != HttpMethod.Post)
                {
                    throw new Exception("Verb Must be POST for Bundle Processing");
                }

                request = new HttpRequestMessage(rm, "");
            }
            else
            {
                if (string.IsNullOrEmpty(rt))
                {
                    throw new Exception("Resource Type not found or is blank in content");
                }

                if (!rt.Equals(reqresource))
                {
                    throw new Exception("Resource Request Type must match resource type in content");
                }

                var id = (string) r["id"];
                if (id == null && rm != HttpMethod.Post)
                {
                    throw new Exception("Must Specify resource id on modification HTTP Verb");
                }

                request = new HttpRequestMessage(rm, rt + (rm != HttpMethod.Post ? "/" + id : ""));
            }

            request.Headers.Add("Accept", "application/json");
            if (BearerToken != null)
            {
                request.Headers.Add("Authorization", "Bearer " + BearerToken);
            }

            AddCustomHeadersToRequest(request, ToHeaderParmArray(headers));
            var srv = r.ToString(Formatting.None);
            request.Content = new StringContent(srv, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(request);

            // Read Response Content (this will usually be JSON content)
            var content = await response.Content.ReadAsStringAsync();

            return new FHIRResponse(content, response.Headers, response.StatusCode);
        }
    }
}
