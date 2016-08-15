using Newtonsoft.Json;
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
        public LinkedInProvider(string clientID, string clientSecret, string scope, NameValueCollection additionalParams = null)
            : base(ProviderType.LinkedIn,
                "https://www.linkedin.com/uas/oauth2/authorization",
                "https://www.linkedin.com/uas/oauth2/accessToken",
                "https://api.linkedin.com/v1/people/~:(id,first-name,last-name,email-address,picture-url,formatted-name,location:(country:(code)),public-profile-url)",
                clientID, clientSecret, "oauth2_access_token", LinkedInProvider.AdditionalParameters(additionalParams))
        {
            if (scope == null)
            {
                Scope = "r_fullprofile r_emailaddress";
            }
            else
            {
                Scope = scope;
            }
        }

        private static NameValueCollection AdditionalParameters(NameValueCollection additionalParams)
        {
            if (additionalParams == null)
            {
                additionalParams = new NameValueCollection();
            }

            additionalParams.Add("format", "json");

            return additionalParams;
        }

        protected override IEnumerable<Claim> GetClaimsFromProfile(Dictionary<string, object> profile)
        {
            var claims = base.GetClaimsFromProfile(profile).ToList();
            var localityClaim = claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Locality));

            var location = Newtonsoft.Json.JsonConvert.DeserializeObject<LinkedInLocation>(localityClaim.Value);

            claims.Remove(localityClaim);

            localityClaim = new Claim(localityClaim.Type, location.Country.Code);

            claims.Add(localityClaim);

            return claims;
        }

        static Dictionary<string, string> supportedClaimTypes = new Dictionary<string, string>();
        static LinkedInProvider()
        {
            supportedClaimTypes.Add("id", ClaimTypes.NameIdentifier);
            supportedClaimTypes.Add("formattedName", ClaimTypes.Name);
            supportedClaimTypes.Add("emailAddress", ClaimTypes.Email);
            supportedClaimTypes.Add("firstName", ClaimTypes.GivenName);
            supportedClaimTypes.Add("lastName", ClaimTypes.Surname);
            supportedClaimTypes.Add("publicProfileUrl", ClaimTypes.Webpage);
            supportedClaimTypes.Add("location", ClaimTypes.Locality);
            supportedClaimTypes.Add("pictureUrl", ClaimTypes.UserData);
        }

        internal override Dictionary<string, string> SupportedClaimTypes
        {
            get { return supportedClaimTypes; }
        }

        private class LinkedInLocation
        {
            [JsonProperty("country")]
            public LinkedInCountry Country { get; set; }
        }

        private class LinkedInCountry
        {
            [JsonProperty("code")]
            public string Code { get; set; }
        }
    }
}
