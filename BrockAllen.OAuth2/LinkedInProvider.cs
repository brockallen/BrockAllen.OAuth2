using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BrockAllen.OAuth2
{
    class LinkedInProvider : Provider
    {
        //scope is not used for now..
        public LinkedInProvider(string clientID, string clientSecret, string scope)
            : base(ProviderType.LinkedIn,
                "https://www.linkedin.com/uas/oauth2/authorization",
                "https://www.linkedin.com/uas/oauth2/accessToken",
                "https://api.linkedin.com/v1/people/~",
                clientID, clientSecret)
        {
            //ignoring the scope for now since where not geting a NameIdentifier using the scope
            if (string.IsNullOrEmpty(scope))
            {
                Scope = "r_basicprofile%20r_emailaddress";
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
            supportedClaimTypes.Add("firstName", ClaimTypes.GivenName);
            supportedClaimTypes.Add("lastName", ClaimTypes.Surname);

        }

        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }

        protected override async Task<IEnumerable<Claim>> GetProfileClaimsAsync(AuthorizationToken token)
        {

            var url = this.ProfileUrl + ":(id,first-name,last-name)?oauth2_access_token=" + token.AccessToken;

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-li-format", "json");
            var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var profile = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return GetClaimsFromProfile(profile);


            }
            return null;
        }
    }
}
