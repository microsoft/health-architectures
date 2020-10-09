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

using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FHIRProxy
{
    internal interface IProxyPreProcess
    {
        /// <summary>
        ///     /* Defines Interface for a ProxyPreProcess Function.  Remember implementations should be as performant as possible and
        ///     thread-safe since instances are reused*/
        /// </summary>
        /// <param name="resquestBody">The requestBody from the function invovation or the previously executed process/filter requestBody</param>
        /// <param name="req">The HttpRequest instance from the Function Invocation</param>
        /// <param name="log">The ILogger instance from the function invocation</param>
        /// <param name="principal">The current ClaimsPrinicipal from the function invocation</param>
        /// <param name="res">The current FHIR ResourceType from the function invocation</param>
        /// <param name="id">The current FHIR Resource Id from the function invocation</param>
        /// <param name="hist">The current FHIR Resource _history path from the function invocation</param>
        /// <param name="vid">The current FHIR Resource version id from the function invocation</param>
        /// <returns>ProxyProcessResult - The result of this Pre Process Function</returns>
        public Task<ProxyProcessResult> Process(string requestBody, HttpRequest req, ILogger log, ClaimsPrincipal principal,
            string res, string id, string hist, string vid);
    }
}
