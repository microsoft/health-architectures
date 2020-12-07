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
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace hl7ingest
{
    public static class hl7ingest
    {
        /* Ingest HL7 via HL7OverHTTP Store raw resource to blob in container.  Then add blob regerence to configured service bus queue (for ordered delivery) to be pickup by conversion API logic app flow.
         * Storage to blob is MessageType MSH-9/year/month/day/hour/specified object id or guid.
         *
         * Blob Binding has to be defined in Environment settings
         * Request should be according to the HAPI HL7OverHTTP Specification: https://hapifhir.github.io/hapi-hl7v2/hapi-hl7overhttp/specification.html
         * Responds with an MSA ACK/NAK message per the Specification
         * 
         */
        private static IQueueClient queueClient;
        private static object _lock = new object();
        [FunctionName("hl7ingest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("%HL7ING-STORAGEACCT-BLOB-CONTAINER%", Connection ="HL7ING-STORAGEACCT")] CloudBlobContainer container,ILogger log)
        {
            string contenttype = req.ContentType;
            log.LogInformation("hl7 ingest HTTP trigger function fired");
            string coid = req.Query["id"];
            if (coid == null) coid = Guid.NewGuid().ToString();
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            JObject jobj = null;
            try
            {
                jobj = HL7ToXmlConverter.ConvertToJObject(requestBody);
                DateTime now = DateTime.Now;
                string msgtype = (string) jobj["hl7message"]["MSH"]["MSH.9"]["MSH.9.1"];
                string msgtrigger = (string)jobj["hl7message"]["MSH"]["MSH.9"]["MSH.9.2"];
                string cntrlid = (string)jobj["hl7message"]["MSH"]["MSH.10"];
                string ds = $"{ msgtype}_{msgtrigger}_{cntrlid}_{coid.ToLower().Replace("-","")}.hl7";
                await container.CreateIfNotExistsAsync();
                string ret = requestBody.Replace('\r', (char)0xA);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(ds);
                await blockBlob.UploadTextAsync(ret);
                var retVal = new ContentResult();
                retVal.ContentType = contenttype;
                retVal.Content = Utilities.GenerateACK(jobj);
                //Create Queue Client if needed
                if (queueClient == null)
                {
                    lock (_lock)
                    {
                        if (queueClient == null)
                        {
                            queueClient = new QueueClient(Utilities.GetEnvironmentVariable("HL7ING-SERVICEBUS-CONNECTION"), Utilities.GetEnvironmentVariable("HL7ING-QUEUENAME","hl7queue"));
                        }   
                    }
                }
                string messageBody = "{\"container\":\"" + Utilities.GetEnvironmentVariable("HL7ING-STORAGEACCT-BLOB-CONTAINER") + "\",\"filename\":\"" + ds +"\",\"msgtype\":\"" + msgtype + "\",\"msgevent\":\"" + msgtrigger + "\",\"ctlid\":\"" + cntrlid + "\"}";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                log.LogInformation($"Sending message: {messageBody} to queue: {Utilities.GetEnvironmentVariable("HL7ING-QUEUENAME","hl7queue")}");
                // Send the message to the queue
                await queueClient.SendAsync(message);
                return retVal;

            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                return new System.Web.Http.InternalServerErrorResult();
            }
        }
    }
}
