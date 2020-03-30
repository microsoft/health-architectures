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
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FHIRProxy
{
    public static class SecureLink
    {
        public static string _bearerToken;
        private static object _lock = new object();
        private static string[] allowedresources = { "patient", "practitioner", "relatedperson" };

        [FunctionName("SecureLink")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "linkresource/{res}/{id}")] HttpRequest req,
            ILogger log, ClaimsPrincipal principal, string res, string id)
        {
            FhirJsonParser _parser = new FhirJsonParser();
            _parser.Settings.AcceptUnknownMembers = true;
            _parser.Settings.AllowUnrecognizedEnums = true;
            log.LogInformation("SecureLink Function Invoked");
            //Is the principal authenticated
            if (!principal.Identity.IsAuthenticated)
            {
                return new ContentResult() { Content = "User is not Authenticated", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
            }
            //Is the prinicipal a FHIR Server Administrator
            ClaimsIdentity ci = (ClaimsIdentity)principal.Identity;

            if (!ci.IsInFHIRRole(Environment.GetEnvironmentVariable("ADMIN_ROLE")))
            {
                return new ContentResult() { Content = "User does not have suffiecient rights (Administrator required)", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
            }
            string aadten = ci.Tenant();
            //Custom Headers for User Audit in FHIR
            List<HeaderParm> auditheaders = new List<HeaderParm>();
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-USERID", principal.Identity.Name));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-TENANT", aadten));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-SOURCE", req.HttpContext.Connection.RemoteIpAddress.ToString()));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-PROXY", "FHIRProxy-LinkResource"));
            //Are we linking the correct resource type
            if (string.IsNullOrEmpty(res) || !allowedresources.Any(res.ToLower().Contains))
            {
                return new BadRequestObjectResult("Linked resource must be Patient,Practitioner or RelatedPerson");
            }
            string name = req.Query["name"];
            if (string.IsNullOrEmpty(name))
            {
                return new BadRequestObjectResult("Linked resource must have principal name specified in parameters (e.g. ?name=)");
            }
            //Get/update/check current bearer token to talk to authenticate to FHIR Server
            if (_bearerToken == null || FHIRClient.isTokenExpired(_bearerToken))
            {
                lock (_lock)
                {
                    log.LogInformation($"Obtaining new OAUTH2 Bearer Token for access to FHIR Server");
                    _bearerToken = FHIRClient.GetOAUTH2BearerToken(System.Environment.GetEnvironmentVariable("FS_TENANT_NAME"), System.Environment.GetEnvironmentVariable("FS_RESOURCE"),
                                                               System.Environment.GetEnvironmentVariable("FS_CLIENT_ID"), System.Environment.GetEnvironmentVariable("FS_SECRET"));
                }
            }
            //Get a FHIR Client so we can talk to the FHIR Server
            log.LogInformation($"Instanciating FHIR Client Proxy");
            FHIRClient fhirClient = new FHIRClient(System.Environment.GetEnvironmentVariable("FS_URL"), _bearerToken);
            int.TryParse(System.Environment.GetEnvironmentVariable("LINK_DAYS"), out int i_link_days);
            //Load the resource to Link
            var fhirresp = fhirClient.LoadResource(res + "/" + id, null, false, auditheaders.ToArray());
            var lres = _parser.Parse<Resource>((string)fhirresp.Content);
            if (lres.ResourceType == Hl7.Fhir.Model.ResourceType.OperationOutcome)
            {

                return new BadRequestObjectResult(lres.ToString());

            }
            //Add Link to AAD Tenent in Identifiers
            if (lres.ResourceType == Hl7.Fhir.Model.ResourceType.Practitioner)
            {
                var tr = (Hl7.Fhir.Model.Practitioner)lres;
                var fbid = tr.Identifier.FirstOrDefault(ident => ident.System == aadten);
                if (fbid != null)
                {
                    tr.Identifier.Remove(fbid);
                }
                Hl7.Fhir.Model.Identifier newid = new Hl7.Fhir.Model.Identifier(aadten, name);
                newid.Period = new Hl7.Fhir.Model.Period(Hl7.Fhir.Model.FhirDateTime.Now(), new Hl7.Fhir.Model.FhirDateTime(DateTimeOffset.Now.AddDays(i_link_days)));
                tr.Identifier.Add(newid);
                FhirJsonSerializer serializer = new FhirJsonSerializer();
                string srv = serializer.SerializeToString(tr);
                var saveresult = fhirClient.SaveResource(Enum.GetName(typeof(ResourceType), tr.ResourceType), srv, "PUT", auditheaders.ToArray());
                if (saveresult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new OkObjectResult($"Identity: {name} in directory {aadten} is now linked to {res}/{id}");

                }
                else
                {
                    return new BadRequestObjectResult($"Unable to link Identity: {name} in directory {aadten}:{saveresult.StatusCode}");
                }
            }
            else if (lres.ResourceType == Hl7.Fhir.Model.ResourceType.Patient)
            {
                var tr = (Hl7.Fhir.Model.Patient)lres;
                var fbid = tr.Identifier.FirstOrDefault(ident => ident.System == aadten);
                if (fbid != null)
                {
                    tr.Identifier.Remove(fbid);
                }
                Hl7.Fhir.Model.Identifier newid = new Hl7.Fhir.Model.Identifier(aadten, name);
                newid.Period = new Hl7.Fhir.Model.Period(Hl7.Fhir.Model.FhirDateTime.Now(), new Hl7.Fhir.Model.FhirDateTime(DateTimeOffset.Now.AddDays(i_link_days)));
                tr.Identifier.Add(newid);
                FhirJsonSerializer serializer = new FhirJsonSerializer();
                string srv = serializer.SerializeToString(tr);
                var saveresult = fhirClient.SaveResource(Enum.GetName(typeof(ResourceType), tr.ResourceType), srv, "PUT", auditheaders.ToArray());
                if (saveresult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new OkObjectResult($"Identity: {name} in directory {aadten} is now linked to {res}/{id}");
                }
                else
                {
                    return new BadRequestObjectResult($"Unable to link Identity: {name} in directory {aadten}:{saveresult.StatusCode}");
                }
            }

            return new OkObjectResult($"No action taken Identity: {name}");

        }

    }
}
