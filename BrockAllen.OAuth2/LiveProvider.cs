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
        public LiveProvider(string clientID, string clientSecret, string scope)
            : base(ProviderType.Live,
                "https://login.live.com/oauth20_authorize.srf",
                "https://login.live.com/oauth20_token.srf",
                "https://apis.live.net/v5.0/me", 
                clientID, clientSecret)
        {
            if (scope == null)
            {
                Scope = "wl.signin%20wl.basic wl.emails";
            }
            else
            {
                Scope = scope;
            }
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
            supportedClaimTypes.Add("locale", ClaimTypes.Locality);
        }
        
        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }

        protected override IEnumerable<Claim> GetClaimsFromProfile(Dictionary<string, object> profile)
        {
            var emailsQuery =
                from item in profile
                where item.Key == "emails"
                select item.Value;
            var emails = emailsQuery.FirstOrDefault();
            if (emails != null)
            {
                profile.Remove("emails");
                var json = emails.ToString();
                var emailsObj = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, new { preferred="" });
                if (emailsObj != null && !String.IsNullOrWhiteSpace(emailsObj.preferred))
                {
                    profile.Add("email", emailsObj.preferred);
                }
            }

            return base.GetClaimsFromProfile(profile);
        }
    }
}
