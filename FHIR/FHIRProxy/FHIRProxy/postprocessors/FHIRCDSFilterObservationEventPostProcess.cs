using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FHIRProxy.postprocessors
{
    class FHIRCDSFilterObservationEventPostProcess : IProxyPostProcess
    {

        public async Task<ProxyProcessResult> Process(FHIRResponse response, HttpRequest req, ILogger log, ClaimsPrincipal principal, string res, string id, string hist, string vid)
        {

            try
            {
                if (req.Method.Equals("POST")
                    && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) && res == "Observation" && req.Headers["X-MS-FHIRCDSSynAgent"] != "true")
                {
                    var fhirresp = JObject.Parse(response.Content.ToString());

                    if (fhirresp["device"] != null && fhirresp["id"] != null)
                    {
                        string ecs = Environment.GetEnvironmentVariable("FP-MOD-BLOB-CONNECTIONSTRING");
                        string enm = "observationdata";
                        await WriteToBlob(response.Content.ToString(), ecs, enm, fhirresp.FHIRResourceId(), log);

                        return new ProxyProcessResult(false, "", "", response);
                    }

                    return new ProxyProcessResult(true, "", "", response);
                }
            }
            catch (Exception exception)
            {
                log.LogError(exception, $"FilterFHIREventPostProcess Exception: {exception.Message}");

            }
            return new ProxyProcessResult(true, "", "", response);
        }

        private async Task WriteToBlob(string fhirresp, string connectionstring, string containername, string id, ILogger log)
        {
            CloudStorageAccount storageAccount;
            CloudStorageAccount.TryParse(connectionstring, out storageAccount);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 'quickstartblobs' and 
            // append a GUID value to it to make the name unique.
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containername);
            await cloudBlobContainer.CreateIfNotExistsAsync();
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(DateTime.UtcNow.ToString("yyyy-MM-dd/") + id + ".json");

            var content = Encoding.UTF8.GetBytes(fhirresp);

            log.LogInformation("Uploading observation to blob");
            await using (var ms = new MemoryStream(content))
            {
                blob.UploadFromStream(ms);
            }
        }
    }
}
