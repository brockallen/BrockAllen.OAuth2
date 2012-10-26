using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    public class CallbackResult
    {
        public string Error { get; set; }
        public string ErrorDetails { get; set; }
        public string ReturnUrl { get; set; }
        public string ProviderName { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
