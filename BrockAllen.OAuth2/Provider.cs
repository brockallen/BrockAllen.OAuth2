﻿using System;
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
            ProviderType type, string scope, 
            string authorizationUrl, string tokenUrl, string profileUrl, 
            string clientID, string clientSecret)
        {
            this.ProviderType = type;
            this.Scope = scope;
            this.AuthorizationUrl = authorizationUrl;
            this.TokenUrl = tokenUrl;
            this.ProfileUrl = profileUrl;
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
        }

        internal abstract Dictionary<string, string> SupportedClaimTypes { get; }

        public AuthorizationRedirect GetRedirect()
        {
            var url = this.AuthorizationUrl;
            var client = this.ClientID;
            var redirect = OAuth2Client.RedirectUrl;
            var state = WebUtility.UrlEncode(Thinktecture.IdentityModel.CryptoRandom.CreateRandomKeyString(10));
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

        public AuthorizationToken GetTokenFromCallback(
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
            if (WebUtility.UrlDecode(ctx.State) != state)
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
            postValues.Add(new KeyValuePair<string, string>("redirect_uri", OAuth2Client.RedirectUrl));
            postValues.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));

            return this.GetTokenFromCallbackInternal(postValues);
        }
        
        protected virtual AuthorizationToken GetTokenFromCallbackInternal(List<KeyValuePair<string, string>> postValues)
        {
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(postValues);
            var result = client.PostAsync(this.TokenUrl, content).Result;
            if (result.IsSuccessStatusCode)
            {
                return ProcessAuthorizationTokenResponse(result);
            }
            else
            {
                var body = result.Content.ReadAsStringAsync().Result;
                return new AuthorizationToken { 
                    Error = "Error contacting token endpoint : " + result.ReasonPhrase,
                    ErrorDetails = body
                };
            }
        }

        protected virtual AuthorizationToken ProcessAuthorizationTokenResponse(HttpResponseMessage result)
        {
            var data = result.Content.ReadAsStringAsync().Result;
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

        protected virtual IEnumerable<Claim> GetProfileClaims(AuthorizationToken token)
        {
            var url = this.ProfileUrl + "?access_token=" + token.AccessToken;
            
            HttpClient client = new HttpClient();
            var result = client.GetAsync(url).Result;
            if (result.IsSuccessStatusCode)
            {
                var json = result.Content.ReadAsStringAsync().Result;
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

        internal CallbackResult ProcessCallback(AuthorizationContext authCtx, NameValueCollection nameValueCollection)
        {
            var token = this.GetTokenFromCallback(authCtx, nameValueCollection);
            if (token.Error != null)
            {
                return new CallbackResult
                {
                    Error = token.Error,
                    ErrorDetails = token.ErrorDetails
                };
            }
            var claims = this.GetProfileClaims(token).ToList(); ;
            var idpClaim = new Claim(IdentityProviderClaimType, this.Name, ClaimValueTypes.String, this.Name);
            claims.Insert(0, idpClaim);
            return new CallbackResult { Claims = claims.ToArray() };
        }
    }
}
