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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


namespace hl7ingest
{
   
    public static class Extensions
    {
        private static Regex REGEX = new Regex(@"\\X[0-9A-F]{2,10}\\");
        private static string parseHex(string hex)
        {

            string s = hex.Substring(2, hex.Length - 3);
            List<char> convert = new List<char>();
            while (s.Length > 1)
            {
                string p = s.Substring(0, 2);
                var c = (char)Int16.Parse(p, NumberStyles.AllowHexSpecifier);
                convert.Add(c);
                s = s.Substring(2);
            }
            return new string(convert.ToArray());

        }
        private static string UnEscapeHL7Hex(string s)
        {
            while (true)
            {
                Match match = REGEX.Match(s);
                if (match.Success)
                {
                    var r = parseHex(match.Value);
                    s = s.Replace(match.Value, r);

                }
                else
                {
                    break;
                }
            }
            return s;
        }
        public static string UnEscapeHL7(this string str)
        {
                var r =  str.Replace("\\T\\", "&").Replace("\\S\\", "^").Replace("\\E\\", "\\").Replace("\\R\\", "~").Replace("\\.br\\","\\n");
                return UnEscapeHL7Hex(r);
        }
        public static string GetFirstField(this JToken o)
        {
            if (o == null) return "";
            if (o.Type == JTokenType.String) return (string)o;
            if (o.Type == JTokenType.Object) return (string)o.First;
            return "";
        }
        public static string GetIPAddress(this HttpRequest Request)
            {
                if (Request.Headers.Keys.Contains("CF-CONNECTING-IP")) return Request.Headers["CF-CONNECTING-IP"].ToString();

                if (Request.Headers.Keys.Contains("HTTP_X_FORWARDED_FOR"))
                {
                    string ipAddress = Request.Headers["HTTP_X_FORWARDED_FOR"];

                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        string[] addresses = ipAddress.Split(',');
                        if (addresses.Length != 0)
                        {
                            return addresses[0];
                        }
                    }
                }

                return Request.Host.ToString();
            }
   
        

    }
}
