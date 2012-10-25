using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace BrockAllen.OAuth2
{
    class AuthorizationToken
    {
        public static AuthorizationToken FromJson(string json)
        {
            var token = JObject.Parse(json);
            var result = new AuthorizationToken();
            result.AccessToken = token.Value<string>("access_token");
            result.TokenType = token.Value<string>("token_type");
            result.RefreshToken = token.Value<string>("refresh_token");
            result.Expiration = DateTime.UtcNow.AddMinutes(token.Value<int>("expires_in"));
            return result;
        }
        
        public static AuthorizationToken FromCollection(NameValueCollection values)
        {
            var result = new AuthorizationToken();
            result.AccessToken = values["access_token"];
            result.TokenType = values["token_type"];
            result.RefreshToken = values["refresh_token"];
            var expires = values["expires_in"];
            int expiresInt;
            if (expires != null && Int32.TryParse(expires, out expiresInt))
            {
                result.Expiration = DateTime.UtcNow.AddMinutes(expiresInt);
            }
            else
            {
                expires = values["expires"];
                if (expires != null && Int32.TryParse(expires, out expiresInt))
                {
                    result.Expiration = DateTime.UtcNow.AddMinutes(expiresInt);
                }
            }
            return result;
        }

        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
        public string Error { get; set; }
        public string ErrorDetails { get; set; }
    }
}
