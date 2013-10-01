using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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
                clientID, clientSecret)
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
            supportedClaimTypes.Add("first-name", ClaimTypes.GivenName);
            supportedClaimTypes.Add("last-name", ClaimTypes.Surname);
            supportedClaimTypes.Add("email-address", ClaimTypes.Email);
        }

        protected async override Task<IEnumerable<Claim>> GetProfileClaimsAsync(AuthorizationToken token)
        {
            var url = this.ProfileUrl;

            //Create Field Selectors for request
            url += ":(";

            for (int i = 0; i < supportedClaimTypes.Count; i++)
            {
                url += supportedClaimTypes.ElementAt(i).Key;
                if ((i + 1) < supportedClaimTypes.Count) url += ",";
            }

            url += ")?oauth2_access_token=" + token.AccessToken;

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get,url);

            //LinkedIn defaults to xml response unless format header is added to request
            request.Headers.Add("x-li-format","json");

            var result = await client.SendAsync(request);
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var profile = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return GetClaimsFromProfile(profile);
            }

            return null;
        }

        protected override IEnumerable<Claim> GetClaimsFromProfile(Dictionary<string, object> profile)
        {
            //Camelize the keys in the dictionary to match LinkedIn's xml to json transformation
            var claimMap = this.SupportedClaimTypes.Camelize();

            var query =
                from key in profile.Keys
                where claimMap.Keys.Contains(key) && profile[key] != null
                select new Claim(claimMap[key], profile[key].ToString(), ClaimValueTypes.String, this.Name);

            return query;
        }

        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }
    }

    public static class LinkedInDictionaryExtension
    {
        public static Dictionary<string, string> Camelize(
            this Dictionary<string, string> dict)
        {
            if (dict.Count.Equals(0)) return dict;

            var nudict = new Dictionary<string, string>(dict.Count);

            foreach (var key in dict.Keys)
            {
                //Remove hyphens and produce camelcased string of words
                var hyphenSubStrings = key.Split('-');
                if (hyphenSubStrings.Length < 1) continue;

                var head = hyphenSubStrings[0];
                var humps = string.Empty;

                //UpperCase first character of remaining substrings
                for (int i = 1; i < hyphenSubStrings.Length; i++)
                {
                    var ss = hyphenSubStrings[i];
                    humps += ss.Substring(0, 1).ToUpper();
                    humps += ss.Substring(1, ss.Length - 1);
                }

                var newKey = head + humps;

                nudict.Add(newKey, dict[key]);
            }

            return nudict;
        }
    }
}
