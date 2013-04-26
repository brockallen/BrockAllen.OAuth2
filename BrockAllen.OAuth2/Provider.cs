using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel;

namespace BrockAllen.OAuth2
{
    abstract class Provider
    {
        internal const string IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        public ProviderType ProviderType { get; set; }
        public string Name
        {
            get
            {
                return ProviderType.ToString();
            }
        }
        public string Scope { get; set; }
        public string AuthorizationUrl { get; set; }
        public string TokenUrl { get; set; }
        public string ProfileUrl { get; set; }

        public string ClientID { get; set; }
        public string ClientSecret { get; set; }

        public Provider(
            ProviderType type, 
            string authorizationUrl, string tokenUrl, string profileUrl, 
            string clientID, string clientSecret)
        {
            this.ProviderType = type;
            this.AuthorizationUrl = authorizationUrl;
            this.TokenUrl = tokenUrl;
            this.ProfileUrl = profileUrl;
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
        }

        string RedirectUrl
        {
            get
            {
                var ctx = System.Web.HttpContext.Current;
                var app = ctx.Request.ApplicationPath;
                if (!app.EndsWith("/")) app += "/";
                var url = new Uri(ctx.Request.Url, app + OAuth2Client.OAuthCallbackUrl);
                return url.AbsoluteUri;
            }
        }

        internal abstract Dictionary<string, string> SupportedClaimTypes { get; }

        public AuthorizationRedirect GetRedirect()
        {
            var url = this.AuthorizationUrl;
            var client = this.ClientID;
            var redirect = RedirectUrl;
            var state = Base64Url.Encode(CryptoRandom.CreateRandomKey(10));
            
            var scope = this.Scope;

            var authorizationUrl = String.Format("{0}?client_id={1}&redirect_uri={2}&state={3}&response_type=code&scope={4}",
                url,
                client,
                redirect,
                state,
                scope);

            var ctx = new AuthorizationRedirect
            {
                AuthorizationUrl = authorizationUrl,
                State = state
            };
            return ctx;
        }

        public async Task<AuthorizationToken> GetTokenFromCallbackAsync(
            AuthorizationContext ctx, 
            NameValueCollection queryString)
        {
            if (ctx.ProviderType != this.ProviderType)
            {
                throw new Exception("Invalid AuthorizationCodeProvider name");
            }

            string error = queryString["error"];
            if (!String.IsNullOrWhiteSpace(error))
            {
                return new AuthorizationToken { Error = "State does not match." };
            }

            string state = queryString["state"];
            if (ctx.State != state)
            {
                return new AuthorizationToken { Error = "State does not match." };
            }

            string code = queryString["code"];
            if (String.IsNullOrWhiteSpace(code))
            {
                return new AuthorizationToken { Error = "Invalid code." };
            }

            List<KeyValuePair<string, string>> postValues =
                new List<KeyValuePair<string, string>>();
            postValues.Add(new KeyValuePair<string, string>("code", code));
            postValues.Add(new KeyValuePair<string, string>("client_id", this.ClientID));
            postValues.Add(new KeyValuePair<string, string>("client_secret", this.ClientSecret));
            postValues.Add(new KeyValuePair<string, string>("redirect_uri", RedirectUrl));
            postValues.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));

            return await this.GetTokenFromCallbackInternalAsync(postValues);
        }
        
        protected virtual async Task<AuthorizationToken> GetTokenFromCallbackInternalAsync(List<KeyValuePair<string, string>> postValues)
        {
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(postValues);
            var result = await client.PostAsync(this.TokenUrl, content);
            if (result.IsSuccessStatusCode)
            {
                return await ProcessAuthorizationTokenResponseAsync(result);
            }
            else
            {
                var body = await result.Content.ReadAsStringAsync();
                return new AuthorizationToken { 
                    Error = "Error contacting token endpoint : " + result.ReasonPhrase,
                    ErrorDetails = body
                };
            }
        }

        protected async virtual Task<AuthorizationToken> ProcessAuthorizationTokenResponseAsync(HttpResponseMessage result)
        {
            var data = await result.Content.ReadAsStringAsync();
            if (result.Content.Headers.ContentType.MediaType.Equals("application/json"))
            {
                // json from body
                return AuthorizationToken.FromJson(data);
            }
            else
            {
                // form-url-encoded from body
                var values = HttpUtility.ParseQueryString(data);
                return AuthorizationToken.FromCollection(values);
            }
        }

        protected async virtual Task<IEnumerable<Claim>> GetProfileClaimsAsync(AuthorizationToken token)
        {
            var url = this.ProfileUrl + "?access_token=" + token.AccessToken;
            
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var profile = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return GetClaimsFromProfile(profile);
            }

            return null;
        }

        protected virtual IEnumerable<Claim> GetClaimsFromProfile(Dictionary<string, object> profile)
        {
            var claimMap = this.SupportedClaimTypes;

            var query = 
                from key in profile.Keys
                where claimMap.Keys.Contains(key) && profile[key] != null
                select new Claim(claimMap[key], profile[key].ToString(), ClaimValueTypes.String, this.Name);
            
            return query;
        }

        internal async Task<CallbackResult> ProcessCallbackAsync(AuthorizationContext authCtx, NameValueCollection nameValueCollection)
        {
            var token = await this.GetTokenFromCallbackAsync(authCtx, nameValueCollection);
            if (token.Error != null)
            {
                return new CallbackResult
                {
                    Error = token.Error,
                    ErrorDetails = token.ErrorDetails
                };
            }
            var result = await this.GetProfileClaimsAsync(token);
            var claims = result.ToList();

            var authInstantClaim = new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s"), ClaimValueTypes.DateTime, this.Name);
            claims.Insert(0, authInstantClaim);
            var idpClaim = new Claim(IdentityProviderClaimType, this.Name, ClaimValueTypes.String, this.Name);
            claims.Insert(0, idpClaim);
            
            return new CallbackResult { Claims = claims.ToArray() };
        }
    }
}
