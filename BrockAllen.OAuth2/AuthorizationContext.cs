using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    public class AuthorizationContext
    {
        public string ProviderType { get; set; }
        public string ReturnUrl { get; set; }
        public string State { get; set; }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        public static AuthorizationContext Parse(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorizationContext>(json);
        }
    }
}
