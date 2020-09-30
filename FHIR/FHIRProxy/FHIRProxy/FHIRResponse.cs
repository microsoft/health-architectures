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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

namespace FHIRProxy
{
    public class FHIRResponse
    {
        public FHIRResponse()
        {
            Headers = new Dictionary<string, HeaderParm>();
        }

        public FHIRResponse(string content, HttpResponseHeaders respheaders, HttpStatusCode status, bool parse = false) : this()
        {
            var filterheaders = Utils
                .GetEnvironmentVariable("FS_RESPONSE_HEADER_NAME", "Date,Last-Modified,ETag,Location,Content-Location")
                .Split(",");
            if (parse)
            {
                Content = JObject.Parse(content);
            }
            else
            {
                Content = content;
            }

            foreach (var head in filterheaders)
            {
                IEnumerable<string> values;
                if (respheaders.TryGetValues(head, out values))
                {
                    Headers.Add(head, new HeaderParm(head, values.First()));
                }
            }

            StatusCode = status;
        }

        public IDictionary<string, HeaderParm> Headers { get; set; }
        public object Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public override string ToString()
        {
            if (Content == null)
            {
                return "";
            }

            if (Content is string)
            {
                return (string) Content;
            }

            if (Content is JToken)
            {
                return ((JToken) Content).ToString();
            }

            return base.ToString();
        }
    }
}
