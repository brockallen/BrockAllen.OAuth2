using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    public class AuthorizationRedirect
    {
        public string AuthorizationUrl { get; set; }
        public string State { get; set; }
    }
}
