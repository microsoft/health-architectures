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
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FHIRProxy
{
    class FHIRProxyAuthorization : FunctionInvocationFilterAttribute
    {
        public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            var req = executingContext.Arguments.First().Value as HttpRequest;
            ClaimsPrincipal principal = executingContext.Arguments["principal"] as ClaimsPrincipal;
            ClaimsIdentity ci = (ClaimsIdentity)principal.Identity;
           
            bool admin = ci.IsInFHIRRole(Environment.GetEnvironmentVariable("ADMIN_ROLE"));
            bool reader = ci.IsInFHIRRole(Environment.GetEnvironmentVariable("READER_ROLE"));
            bool writer = ci.IsInFHIRRole(Environment.GetEnvironmentVariable("WRITER_ROLE"));
            string inroles = "";
            if (admin) inroles += "A";
            if (reader) inroles += "R";
            if (writer) inroles += "W";
            req.Headers.Remove(Utils.AUTH_STATUS_HEADER);
            req.Headers.Remove(Utils.AUTH_STATUS_MSG_HEADER);
            if (!principal.Identity.IsAuthenticated)
            {
                req.Headers.Add(Utils.AUTH_STATUS_HEADER, ((int)System.Net.HttpStatusCode.Unauthorized).ToString());
                req.Headers.Add(Utils.AUTH_STATUS_MSG_HEADER, "User is not Authenticated");

            }
            else if (req.Method.Equals("GET") && !admin && !reader)
            {
                req.Headers.Add(Utils.AUTH_STATUS_HEADER, ((int)System.Net.HttpStatusCode.Unauthorized).ToString());
                req.Headers.Add(Utils.AUTH_STATUS_MSG_HEADER, "User/Application must be in a reader role to access");

            }
            else if (!req.Method.Equals("GET") && !admin && !writer)
            {
                req.Headers.Add(Utils.AUTH_STATUS_HEADER, ((int)System.Net.HttpStatusCode.Unauthorized).ToString());
                req.Headers.Add(Utils.AUTH_STATUS_MSG_HEADER, "User/Application must be in a writer role to update");

            }
            else
            {
                //Since we are proxying with service client need to ensure authenticated proxy principal is audited
                req.Headers.Add(Utils.AUTH_STATUS_HEADER, ((int)System.Net.HttpStatusCode.OK).ToString());
                req.Headers.Add(Utils.FHIR_PROXY_ROLES, inroles);
                req.Headers.Add("X-MS-AZUREFHIR-AUDIT-USERID", principal.Identity.Name);
                req.Headers.Add("X-MS-AZUREFHIR-AUDIT-TENANT", ci.Tenant());
                req.Headers.Add("X-MS-AZUREFHIR-AUDIT-SOURCE", req.HttpContext.Connection.RemoteIpAddress.ToString());
                req.Headers.Add("X-MS-AZUREFHIR-AUDIT-PROXY", $"{executingContext.FunctionName}");
               
            }
          
            return base.OnExecutingAsync(executingContext, cancellationToken);
        }


       
    }
}
