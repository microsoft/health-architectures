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
using Fhir.Anonymizer.Core;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Logging.Abstractions;

namespace FHIRProxy
{
    public static class ParticipantAccess
    {
        /*  The Participant FHIR proxy by default only validates that the user principal is authenticated (AuthN).
           *You can clone this code and add your own authorization logic, pre and post processing logic to fit your business
           *use cases. This function will accept standard REST Verbs as used by HL7 FHIR
           * 
           * IMPORTANT:  Do not publish this function without Authentication (Easy Auth or APIM) you will compromise your FHIR server!
           * 
          */
        private static string _bearerToken = null;
        private static AnonymizerConfigurationManager _deidconfig = null;
        private static object _lock = new object();
        [FunctionName("ParticipantAccess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "delete", Route = "participant/{res}/{id?}")] HttpRequest req,
            [Blob("%DEIDCONFIG%", FileAccess.Read, Connection = "STORAGEACCT")] CloudBlockBlob deidconfig,
                         ILogger log, ClaimsPrincipal principal, string res, string id)
        {
            log.LogInformation("FHIR SecureAccess Function Invoked");
            if (!principal.Identity.IsAuthenticated)
            {
                return new ContentResult() { Content = "User is not Authenticated", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
            }
            //Is the prinicipal a FHIR Server Administrator
            ClaimsIdentity ci = (ClaimsIdentity)principal.Identity;
            bool admin = ci.IsInFHIRRole(Environment.GetEnvironmentVariable("ADMIN_ROLE"));
            //GET (READ)
            
            if (req.Method.Equals("GET"))
            {
                if (!admin && !ci.IsInFHIRRole(Environment.GetEnvironmentVariable("READER_ROLE")))
                {
                    return new ContentResult() { Content = "User does not have suffiecient rights (READER is required)", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
                }
            }
            else
            { //OTHER VERBS ARE WRITER
                if (!admin && !ci.IsInFHIRRole(Environment.GetEnvironmentVariable("WRITER_ROLE")))
                {
                    return new ContentResult() { Content = "User does not have suffiecient rights (WRITER is required)", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
                }
            }
            string aadten = ci.Tenant();
            string name = principal.Identity.Name;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //Get/update/check current bearer token to talk to authemticate to FHIR Server
            if (_bearerToken == null || FHIRClient.isTokenExpired(_bearerToken))
            {
                lock (_lock)
                {
                    if (_bearerToken == null || FHIRClient.isTokenExpired(_bearerToken))
                    {
                        log.LogInformation($"Obtaining new OAUTH2 Bearer Token for access to FHIR Server");
                        _bearerToken = FHIRClient.GetOAUTH2BearerToken(System.Environment.GetEnvironmentVariable("FS_TENANT_NAME"), System.Environment.GetEnvironmentVariable("FS_RESOURCE"),
                                                               System.Environment.GetEnvironmentVariable("FS_CLIENT_ID"), System.Environment.GetEnvironmentVariable("FS_SECRET"));
                    }
                }
            }
            //Create User Custom Headers for Audit
            List<HeaderParm> auditheaders = new List<HeaderParm>();
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-USERID", name));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-TENANT", aadten));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-SOURCE", req.HttpContext.Connection.RemoteIpAddress.ToString()));
            auditheaders.Add(new HeaderParm("X-MS-AZUREFHIR-AUDIT-PROXY", "FHIRProxy-ParticipantPatientRelationship"));
            //Preserve Relevant FHIR Headers
            List<HeaderParm> customandrestheaders = new List<HeaderParm>();
            foreach (string key in req.Headers.Keys)
            {
                string s = key.ToLower();
                if (s.Equals("etag")) customandrestheaders.Add(new HeaderParm(key, req.Headers[key].First()));
                else if (s.StartsWith("if-"))
                {
                    customandrestheaders.Add(new HeaderParm(key, req.Headers[key]
                        .First()));
                }
            }
            //Add User Audit Headers
            customandrestheaders.AddRange(auditheaders);
            //Get a FHIR Client so we can talk to the FHIR Server
            log.LogInformation($"Instanciating FHIR Client Proxy");
            FHIRClient fhirClient = new FHIRClient(System.Environment.GetEnvironmentVariable("FS_URL"), _bearerToken);
            FHIRResponse fhirresp = null;
            List<string> resourceidentities = new List<string>();
            List<string> inroles = ci.Roles();
            List<string> fhirresourceroles = new List<string>();
            fhirresourceroles.AddRange(Environment.GetEnvironmentVariable("PARTICIPANT_ACCESS_ROLES").Split(","));
            fhirresourceroles.AddRange(Environment.GetEnvironmentVariable("PATIENT_ACCESS_ROLES").Split(","));
            //Load linked Resource Identifiers for each known role the user is in
            foreach (string r in inroles)
            {
                if (fhirresourceroles.Any(r.Equals))
                {
                    fhirresp = fhirClient.LoadResource(r, $"identifier={aadten}|{name}", true, auditheaders.ToArray());
                    var st = (JObject)fhirresp.Content;
                    if (st !=null && ((string)st["resourceType"]).Equals("Bundle"))
                    {
                        JArray entries = (JArray)st["entry"];
                        foreach (JToken tok in entries)
                        {
                            resourceidentities.Add((string)tok["resource"]["resourceType"] + "/" + (string)tok["resource"]["id"]);
                        }
                    }
                }
            }
            //Proxy the call to the FHIR Server 
            JObject result = null;
            Dictionary<string, bool> porcache = new Dictionary<string, bool>();
            if (req.Method.Equals("GET"))
            {
                var qs = req.QueryString.HasValue ? req.QueryString.Value : null;
                fhirresp = fhirClient.LoadResource(res + (id == null ? "" : "/" + id), qs, false, customandrestheaders.ToArray());
            }
            else
            {
                fhirresp = fhirClient.SaveResource(requestBody, req.Method, customandrestheaders.ToArray());
            }
            //Fix location header to proxy address
            if (fhirresp.Headers.ContainsKey("Location"))
            {
                fhirresp.Headers["Location"].Value = fhirresp.Headers["Location"].Value.Replace(Environment.GetEnvironmentVariable("FS_URL"), req.Scheme + "://" + req.Host.Value + req.Path.Value.Substring(0, req.Path.Value.IndexOf(res) - 1));
            }
            var fhirstr = fhirresp.Content == null ? "" : (string)fhirresp.Content;
            //Fix server locations to proxy address
            fhirstr = fhirstr.Replace(Environment.GetEnvironmentVariable("FS_URL"), req.Scheme + "://" + req.Host.Value + req.Path.Value.Substring(0, req.Path.Value.IndexOf(res) - 1));
            result = JObject.Parse(fhirstr);
            //Role Checks if not Administrator
            if (!admin && !ci.IsInFHIRRole(Environment.GetEnvironmentVariable("GLOBAL_ACCESS_ROLES")))
            {
                if (((string)result["resourceType"]).Equals("Bundle"))
                {
                    JArray entries = (JArray)result["entry"];
                    JArray toremove = new JArray();
                    for (int i = entries.Count - 1; i >= 0; i--)
                    {
                        if (!IsAParticipantOrPatient((JObject)entries[i]["resource"], fhirClient, resourceidentities, porcache, auditheaders.ToArray())) entries[i].Remove();

                    }
                }
                else if (!((string)result["resourceType"]).Equals("OperationalOutcome"))
                {
                    if (!IsAParticipantOrPatient(result, fhirClient, resourceidentities, porcache, auditheaders.ToArray()))
                    {
                        return new ContentResult() { Content = $"Not authorized to access resource:{res + (id == null ? "" : "/" + id)}", StatusCode = (int)System.Net.HttpStatusCode.Unauthorized };
                    }
                }
            }
            //Add Response from FHIR Server Headers
            foreach (string key in fhirresp.Headers.Keys)
            {
                req.HttpContext.Response.Headers.Remove(key);
                req.HttpContext.Response.Headers.Add(key, fhirresp.Headers[key].Value);
            }
            //DE-ID
            if (ci.IsInFHIRRole(Environment.GetEnvironmentVariable("DEID_ROLES")))
            {
                if (_deidconfig == null && deidconfig.ExistsAsync().GetAwaiter().GetResult())
                {
                    lock (_lock)
                    {
                        if (_deidconfig == null)
                        {
                            log.LogInformation($"Loading de-id config from blob store");
                            var cs = deidconfig.DownloadTextAsync().GetAwaiter().GetResult();
                            _deidconfig = AnonymizerConfigurationManager.CreateFromConfigurationString(cs);
                        }
                    }
                }
                AnonymizerEngine _engine = new AnonymizerEngine(_deidconfig);
                var str1 = _engine.AnonymizeJson(result.ToString(Formatting.None));
                result = JObject.Parse(str1);
               
            }
            return new JsonResult(result);
        }

        private static bool IsAParticipantOrPatient(JObject resource, FHIRClient fhirClient, IEnumerable<string> knownresourceIdentities, Dictionary<string, bool> porcache, HeaderParm[] auditheaders)
        {

            string patientId = null;
            string encounterId = null;
            JObject patient = null;
            JObject encounter = null;

            string rt = (string)resource["resourceType"];
            //Check for Patient resource or load patient resource from subject member
            if (rt.Equals("Patient"))
            {
                patient = resource;
                patientId = rt + "/" + (string)resource["id"];
            }
            if (patient == null)
            {
                patientId = (string)resource?["subject"]?["reference"];
            }
            if (rt.Equals("Encounter"))
            {
                encounter = resource;
                encounterId = rt + "/" + (string)resource["id"];
                patientId = (string)resource?["subject"]?["reference"];
            }
            if (encounter == null)
            {
                encounterId = (string)resource?["encounter"]?["reference"];
            }
            //If no patient or encounter records present assume not tied to patient do not filter;
            if (patientId == null && encounterId == null) return true;
            //See if patientId is in POR Cache
            if (!string.IsNullOrEmpty(patientId) && porcache.ContainsKey(patientId)) return porcache[patientId];
            if (!string.IsNullOrEmpty(encounterId) && porcache.ContainsKey(encounterId)) return porcache[encounterId];
            //Load the patient if needed
            if (patient == null)
            {
                if (!string.IsNullOrEmpty(patientId))
                {
                    var pat = fhirClient.LoadResource(patientId, null, false, auditheaders);
                    JObject temp = JObject.Parse((string)pat.Content);
                    if (temp != null && ((string)temp["resourceType"]).Equals("Patient"))
                    {
                        patient = temp;
                    }
                    else
                    {
                        porcache[patientId] = false;
                        return false;
                    }

                }
                else if (!string.IsNullOrEmpty(encounterId) && patient == null)
                {
                    var enc = fhirClient.LoadResource(encounterId, null, false, auditheaders);
                    if (enc != null)
                    {
                        JObject temp = JObject.Parse((string)enc.Content);
                        if (temp != null && ((string)temp["resourceType"]).Equals("Encounter") && (string)temp["subject"]?["reference"] != null)
                        {
                            patientId = (string)temp["subject"]?["reference"];
                            var pat = fhirClient.LoadResource(patientId, null, false, auditheaders);
                            JObject temp1 = JObject.Parse((string)pat.Content);
                            if (temp1 != null && ((string)temp1["resourceType"]).Equals("Patient"))
                            {
                                patient = temp1;
                            }
                            else
                            {
                                porcache[patientId] = false;
                                return false;
                            }
                        }
                        else
                        {
                            porcache[encounterId] = false;
                            return false;
                        }
                    }

                }
                else
                {
                    //Cannot Determine/Find a Patient or Encounter reference assume it's not a patient reference
                    return true;
                }
            }


            foreach (string rid in knownresourceIdentities)
            {
                if (rid.StartsWith("Patient"))
                {
                    string pid = rid.Split("/")[1];
                    if (pid.Equals(patientId))
                    {
                        porcache[patientId] = true;
                        if (!string.IsNullOrEmpty(encounterId)) porcache[encounterId] = true;
                        return true;
                    }
                }
                else if (rid.StartsWith("Practitioner"))
                {
                    if (patient["generalPractitioner"] != null)
                    {
                        var gp_s = from gp in patient["generalPractitioner"]
                                   where (string)gp["reference"] == rid
                                   select (string)gp["reference"];
                        if (gp_s != null && gp_s.Count() > 0)
                        {
                            porcache[patientId] = true;
                            if (!string.IsNullOrEmpty(encounterId)) porcache[encounterId] = true;
                            return true;
                        }

                    }
                    string pid = rid.Split("/")[1];
                    string patid = (string)patient["id"];
                    var porencs = fhirClient.LoadResource("Encounter", $"patient={patid}&participant={pid}", false, auditheaders);
                    if (porencs != null)
                    {
                        JObject temp2 = JObject.Parse((string)porencs.Content);
                        if (temp2 != null && ((string)temp2["resourceType"]).Equals("Bundle"))
                        {
                            JArray entries = (JArray)temp2["entry"];
                            if (entries != null && entries.Count > 0)
                            {
                                porcache[patientId] = true;
                                if (!string.IsNullOrEmpty(encounterId)) porcache[encounterId] = true;
                                return true;
                            }
                        }
                    }
                }
            }
            porcache[patientId] = false;
            if (!string.IsNullOrEmpty(encounterId)) porcache[encounterId] = false;
            return false;
        }
    }
}
