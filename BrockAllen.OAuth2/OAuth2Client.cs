using Newtonsoft.Json.Linq;
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

namespace BrockAllen.OAuth2
{
    public class OAuth2Client
    {
        public static string OAuthCallbackUrl { get; set; }
        public static bool AutoRegisterOAuthCallbackUrl { get; set; }
        public static string AuthorizationContextCookieName { get; set; }
        internal static string RedirectUrl
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

        static OAuth2Client()
        {
            AutoRegisterOAuthCallbackUrl = true;
            OAuthCallbackUrl = "oauth2callback";
            AuthorizationContextCookieName = "oauth2authctx";
        }

        static System.Collections.Concurrent.ConcurrentDictionary<ProviderType, Provider> providers =
            new System.Collections.Concurrent.ConcurrentDictionary<ProviderType, Provider>();

        public static void RegisterProvider(ProviderType providerType, string clientID, string clientSecret)
        {
            Provider provider = null;
            switch (providerType)
            {
                case ProviderType.Google:
                    provider = new GoogleProvider(clientID, clientSecret);
                    break;
                case ProviderType.Live:
                    provider = new LiveProvider(clientID, clientSecret);
                    break;
                case ProviderType.Facebook:
                    provider = new FacebookProvider(clientID, clientSecret);
                    break;
            }

            if (provider == null)
            {
                throw new ArgumentException("Invalid provider type");
            }

            providers[providerType] = provider;
        }

        internal static Provider GetProvider(ProviderType providerType)
        {
            var provider = providers[providerType];
            if (provider == null)
            {
                throw new ArgumentException("Invalid provider type");
            }
            return provider;
        }

        internal static void RedirectToAuthorizationProvider(
            ProviderType providerType, 
            System.Web.HttpContextBase ctx, 
            string returnUrl)
        {
            var provider = GetProvider(providerType);

            var redirect = provider.GetRedirect();
            var authCtx = new AuthorizationContext
            {
                ProviderType = providerType, 
                ReturnUrl = returnUrl, 
                State = redirect.State
            };
            SaveContext(authCtx, ctx);

            ctx.Response.Redirect(redirect.AuthorizationUrl);
        }

        static void SaveContext(AuthorizationContext authCtx, HttpContextBase ctx)
        {
            var json = authCtx.ToJson();
            var cookie = new HttpCookie(AuthorizationContextCookieName, json);
            cookie.Secure = ctx.Request.IsSecureConnection;
            cookie.HttpOnly = true;
            cookie.Path = ctx.Request.ApplicationPath;
            ctx.Response.Cookies.Add(cookie);
        }

        static AuthorizationContext GetContext(HttpContextBase ctx)
        {
            var cookie = ctx.Request.Cookies[AuthorizationContextCookieName];
            if (cookie == null) return null;
            var authCtx = AuthorizationContext.Parse(cookie.Value);

            cookie = new HttpCookie(AuthorizationContextCookieName, ".");
            cookie.Secure = ctx.Request.IsSecureConnection;
            cookie.HttpOnly = true;
            cookie.Path = ctx.Request.ApplicationPath;
            cookie.Expires = DateTime.UtcNow.AddYears(-1);
            ctx.Response.Cookies.Add(cookie);

            return authCtx;
        }

        internal static CallbackResult ProcessCallback(HttpContextBase ctx)
        {
            var authCtx = GetContext(ctx);
            if (authCtx == null)
            {
                return new CallbackResult 
                { 
                    Error = "No Authorization Context Cookie" 
                };
            }
            var provider = GetProvider(authCtx.ProviderType);
            var result = provider.ProcessCallback(authCtx, ctx.Request.QueryString);
            result.ReturnUrl = authCtx.ReturnUrl;
            result.ProviderName = authCtx.ProviderType.ToString();
            return result;
        }
    }
}
