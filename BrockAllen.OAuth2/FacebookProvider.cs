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
        public FacebookProvider(string clientID, string clientSecret, string scope)
            : base(ProviderType.Facebook,                
                "https://www.facebook.com/dialog/oauth",
                "https://graph.facebook.com/oauth/access_token",
                "https://graph.facebook.com/me",
                clientID, clientSecret)
        {
            if (scope == null)
            {
                Scope = "email";
            }
            else
            {
                Scope = scope;
            }
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static FacebookProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("name", ClaimTypes.Name);
            supportedClaimTypes.Add("first_name", ClaimTypes.GivenName);
            supportedClaimTypes.Add("last_name", ClaimTypes.Surname);
            supportedClaimTypes.Add("gender", ClaimTypes.Gender);
            supportedClaimTypes.Add("link", ClaimTypes.Webpage);
            supportedClaimTypes.Add("birthday", ClaimTypes.DateOfBirth);
            supportedClaimTypes.Add("locale", ClaimTypes.Locality);
            supportedClaimTypes.Add("email", ClaimTypes.Email);
        }
        
        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }
    }
}
