using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    class GoogleProvider : Provider
    {
        public GoogleProvider(string clientID, string clientSecret)
            : base(ProviderType.Google,
                "https://www.googleapis.com/auth/userinfo.profile",
                "https://accounts.google.com/o/oauth2/auth",
                "https://accounts.google.com/o/oauth2/token",
                "https://www.googleapis.com/oauth2/v1/userinfo",
                clientID, clientSecret)
        {
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static GoogleProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("name", ClaimTypes.Name);
            supportedClaimTypes.Add("email", ClaimTypes.Email);
            supportedClaimTypes.Add("given_name", ClaimTypes.GivenName);
            supportedClaimTypes.Add("family_name", ClaimTypes.Surname);
            supportedClaimTypes.Add("gender", ClaimTypes.Gender);
        }
        
        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }
    }
}
