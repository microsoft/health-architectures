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
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace FHIRProxy
{
    public static class Extensions
    {
        public static bool IsInFHIRRole(this ClaimsIdentity identity, string rolestring)
        {
            if (string.IsNullOrEmpty(rolestring)) return false;
            string[] roles = rolestring.Split(",");
            foreach(string r in roles)
            {
                if (identity.Roles().Exists(s => s.Equals(r)))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<string> Roles(this ClaimsIdentity identity)
        {
            return identity.Claims
                           .Where(c => c.Type == "roles")
                           .Select(c => c.Value)
                           .ToList();
        }
        public static string Tenant(this ClaimsIdentity identity)
        {
            var tid = identity.Claims
                           .Where(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid");
            if (!tid.Any())
            {
                return "";
            } else
            {
                return $"http://schemas.microsoft.com/identity/claims/tenantid/{tid.Single().Value}"; 
            }
                           
            
        }
    }
}
