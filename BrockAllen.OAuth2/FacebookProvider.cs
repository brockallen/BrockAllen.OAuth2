using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrockAllen.OAuth2
{
    class FacebookProvider : Provider
    {
        public FacebookProvider(string clientID, string clientSecret)
            : base(ProviderType.Facebook,
                "",
                "https://www.facebook.com/dialog/oauth",
                "https://graph.facebook.com/oauth/access_token",
                "https://graph.facebook.com/me",
                clientID, clientSecret)
        {
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static FacebookProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("name", ClaimTypes.Name);
            supportedClaimTypes.Add("first_name", ClaimTypes.GivenName);
            supportedClaimTypes.Add("last_name", ClaimTypes.Surname);
            supportedClaimTypes.Add("gender", ClaimTypes.Gender);
        }
        
        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }
    }
}
