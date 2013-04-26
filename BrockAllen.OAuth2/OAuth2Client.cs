using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
using System.Web.Security;

namespace BrockAllen.OAuth2
{
    public class OAuth2Client
    {
        public static string OAuthCallbackUrl { get; set; }
        public static bool AutoRegisterOAuthCallbackUrl { get; set; }
        public static string AuthorizationContextCookieName { get; set; }

        static OAuth2Client instance = new OAuth2Client();
        public static OAuth2Client Instance
        {
            get { return instance; }
        }

        static OAuth2Client()
        {
            AutoRegisterOAuthCallbackUrl = false;
            OAuthCallbackUrl = "oauth2callback";
            AuthorizationContextCookieName = "oauth2authctx";
        }

        private OAuth2Client()
        {
        }
        public OAuth2Client(ProviderType providerType, string clientID, string clientSecret, string scope = null)
        {
            this.RegisterProvider(providerType, clientID, clientSecret, scope);
        }

        ConcurrentDictionary<ProviderType, Provider> providers = new ConcurrentDictionary<ProviderType, Provider>();

        public void RegisterProvider(ProviderType providerType, string clientID, string clientSecret, string scope = null)
        {
            Provider provider = null;
            switch (providerType)
            {
                case ProviderType.Google:
                    provider = new GoogleProvider(clientID, clientSecret, scope);
                    break;
                case ProviderType.Live:
                    provider = new LiveProvider(clientID, clientSecret, scope);
                    break;
                case ProviderType.Facebook:
                    provider = new FacebookProvider(clientID, clientSecret, scope);
                    break;
            }

            if (provider == null)
            {
                throw new ArgumentException("Invalid provider type");
            }

            providers[providerType] = provider;
        }

        internal Provider GetProvider(ProviderType providerType)
        {
            var provider = providers[providerType];
            if (provider == null)
            {
                throw new ArgumentException("Invalid provider type");
            }
            return provider;
        }

        public void RedirectToAuthorizationProvider(
            ProviderType providerType, string returnUrl = null)
        {
            var provider = this.GetProvider(providerType);

            var redirect = provider.GetRedirect();
            var authCtx = new AuthorizationContext
            {
                ProviderType = providerType, 
                ReturnUrl = returnUrl, 
                State = redirect.State
            };
            SaveContext(authCtx);

            var ctx = HttpContext.Current;
            ctx.Response.Redirect(redirect.AuthorizationUrl);
        }

        public async Task<CallbackResult> ProcessCallbackAsync()
        {
            var authCtx = GetContext();
            if (authCtx == null)
            {
                return new CallbackResult 
                { 
                    Error = "No Authorization Context Cookie" 
                };
            }
            var provider = GetProvider(authCtx.ProviderType);
            var ctx = HttpContext.Current;
            var result = await provider.ProcessCallbackAsync(authCtx, ctx.Request.QueryString);
            result.ReturnUrl = authCtx.ReturnUrl;
            result.ProviderName = authCtx.ProviderType.ToString();
            return result;
        }
        
        void SaveContext(AuthorizationContext authCtx)
        {
            var ctx = HttpContext.Current;
            
            var json = authCtx.ToJson();
            var data = Protect(Encoding.UTF8.GetBytes(json));
            
            var cookie = new HttpCookie(AuthorizationContextCookieName, data);
            cookie.Secure = ctx.Request.IsSecureConnection;
            cookie.HttpOnly = true;
            cookie.Path = ctx.Request.ApplicationPath;
            ctx.Response.Cookies.Add(cookie);
        }

        AuthorizationContext GetContext()
        {
            var ctx = HttpContext.Current;
            var cookie = ctx.Request.Cookies[AuthorizationContextCookieName];
            if (cookie == null) return null;
            
            var json = Encoding.UTF8.GetString(Unprotect(cookie.Value));
            var authCtx = AuthorizationContext.Parse(json);

            cookie = new HttpCookie(AuthorizationContextCookieName, ".");
            cookie.Secure = ctx.Request.IsSecureConnection;
            cookie.HttpOnly = true;
            cookie.Path = ctx.Request.ApplicationPath;
            cookie.Expires = DateTime.UtcNow.AddYears(-1);
            ctx.Response.Cookies.Add(cookie);

            return authCtx;
        }

        const string MachineKeyPurpose = "BrockAllen.OAuth2";
        string Protect(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            var value = MachineKey.Protect(data, MachineKeyPurpose);
            return Convert.ToBase64String(value);
        }

        byte[] Unprotect(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            var bytes = Convert.FromBase64String(value);
            return MachineKey.Unprotect(bytes, MachineKeyPurpose);
        }
    }
}
