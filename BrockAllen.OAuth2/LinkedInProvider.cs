using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    class LinkedInProvider : Provider
    {
        public LinkedInProvider(string clientID, string clientSecret, string scope)
            : base(ProviderType.LinkedIn,
                "https://www.linkedin.com/uas/oauth2/authorization",
                "https://www.linkedin.com/uas/oauth2/accessToken",
                "https://api.linkedin.com/v1/people/~",
                clientID, clientSecret, "oauth2_access_token", new NameValueCollection(){ { "format", "json" } })
        {
            if (scope == null)
            {
                Scope = "r_basicprofile r_emailaddress";
            }
            else
            {
                Scope = scope;
            }
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static LinkedInProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("formatted-name", ClaimTypes.Name);
            supportedClaimTypes.Add("email", ClaimTypes.Email);
            supportedClaimTypes.Add("first-name", ClaimTypes.GivenName);
            supportedClaimTypes.Add("last-name", ClaimTypes.Surname);
            supportedClaimTypes.Add("public-profile-url", ClaimTypes.Webpage);
            supportedClaimTypes.Add("location:(country:(code))", ClaimTypes.Locality);
            supportedClaimTypes.Add("picture-url", ClaimTypes.UserData);
        }
        
        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }
    }
}
