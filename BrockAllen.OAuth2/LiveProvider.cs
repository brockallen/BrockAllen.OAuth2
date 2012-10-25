using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    class LiveProvider : Provider
    {
        public LiveProvider(string clientID, string clientSecret)
            : base(ProviderType.Live,
                "wl.signin%20wl.basic",
                "https://login.live.com/oauth20_authorize.srf",
                "https://login.live.com/oauth20_token.srf",
                "https://apis.live.net/v5.0/me", 
                clientID, clientSecret)
        {
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static LiveProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("name", ClaimTypes.Name);
            supportedClaimTypes.Add("email", ClaimTypes.Email);
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
