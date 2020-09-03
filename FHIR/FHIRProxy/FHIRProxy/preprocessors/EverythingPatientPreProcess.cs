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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FHIRProxy.preprocessors
{
    //Implements a non-pageable patient level $everything with the first 100 related entries for the following Resources:Appointment,CarePlan,Condition,DiagnosticReport,Encounter,Immunization,MedicationRequest,Observation,Procedure
    class EverythingPatientPreProcess : IProxyPreProcess
    {
        public static string[] everyresource = { "Appointment:patient={id}","CarePlan:patient={id}","Condition:patient={id}","DiagnosticReport:subject={id}","Encounter:patient={id}","Immunization:patient={id}","MedicationRequest:patient={id}","Observation:patient={id}", "Procedure:patient={id}" };
        
        public async Task<ProxyProcessResult> Process(string requestBody, HttpRequest req, ILogger log, ClaimsPrincipal principal, string res, string id, string hist, string vid)
        {
            if (req.Method.Equals("GET") && res.SafeEquals("Patient") && !string.IsNullOrEmpty(id) && hist.SafeEquals("$everything")) {
                ConcurrentBag<JToken> ss = new ConcurrentBag<JToken>();
                FHIRClient fhirClient = FHIRClientFactory.getClient(log);
                var nextresult = await fhirClient.LoadResource("Patient","_id=" + id);
                var fhirresp = JObject.Parse(nextresult.Content.ToString());
                if (fhirresp.IsNullOrEmpty() || !fhirresp.FHIRResourceType().Equals("Bundle") || !((string)fhirresp["type"]).Equals("searchset")) return new ProxyProcessResult(false, "Patient not found", "", nextresult);
                addEntries((JArray)fhirresp["entry"], ss, log,"Patient");
                CountdownEvent countdown = new CountdownEvent(everyresource.Length);
                foreach (string rt in everyresource)
                {
                    ThreadPool.QueueUserWorkItem(async delegate
                    {
                        try
                        {
                            FHIRClient fhirClient1 = FHIRClientFactory.getClient(log);
                            string[] s = rt.Split(":");
                            log.LogInformation($"Loading {s[0]} resources for patient {id}");
                            var rslt = await fhirClient1.LoadResource(s[0], s[1].Replace("{id}", id) + "&_count=100");
                            var resp = JObject.Parse(rslt.Content.ToString());
                            if (!resp.IsNullOrEmpty() && resp.FHIRResourceType().Equals("Bundle") && ((string)resp["type"]).Equals("searchset"))
                            {
                                addEntries((JArray)resp["entry"], ss, log, s[0]);
                            }
                            countdown.Signal();

                        }
                        catch (Exception e)
                        {
                               log.LogError($"Error fetching {rt} resources for patient {id}:{e.Message}");
                               countdown.Signal();

                        }
                        });

                    }
                countdown.Wait();
                fhirresp["entry"] = new JArray(ss.ToArray());
                fhirresp["link"] = new JArray();
                nextresult.Content = fhirresp;
                return new ProxyProcessResult(false, "", requestBody, nextresult);
            }
            return new ProxyProcessResult(true,"",requestBody,null);
        }
        private void addEntries(JArray entries, ConcurrentBag<JToken> ss, ILogger log,string resname)
        {
            if (!entries.IsNullOrEmpty())
            {
                log.LogInformation($"Adding {entries.Count} {resname} bundle entries to everything array...");
                foreach(JToken tok in entries)
                {
                    ss.Add(tok);
                }
            }
        }
    }
}
