using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace BrockAllen.OAuth2
{
    public class OAuthAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "OAuth2"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            if (OAuth2Client.AutoRegisterOAuthCallbackUrl)
            {
                context.MapRoute("OAuth2Client-CodeCallback",
                    OAuth2Client.OAuthCallbackUrl,
                    new { controller = "OAuth2Callback", action = "Callback" });

                var settings = ConfigurationManager.AppSettings;
                var providers = from v in
                                    (from string q in settings.Keys
                                     where q.StartsWith("oauth2:")
                                     let parts = q.Split(':')
                                     where parts.Length == 3
                                     select new { provider=parts[1], key = parts[2], value=settings[q] })
                                group v by v.provider;
                foreach (var provider in providers)
                {
                    try
                    {
                        var type = (ProviderType)Enum.Parse(typeof(ProviderType), provider.Key, true);
                        var clientID = provider.Where(x => x.key.Equals("clientID", StringComparison.OrdinalIgnoreCase)).Select(x => x.value).SingleOrDefault();
                        var clientSecret = provider.Where(x => x.key.Equals("clientSecret", StringComparison.OrdinalIgnoreCase)).Select(x => x.value).SingleOrDefault();
                        var scope = provider.Where(k => k.key.Equals("scope", StringComparison.OrdinalIgnoreCase)).Select(x=>x.value).SingleOrDefault();
                        
                        if (!String.IsNullOrWhiteSpace(clientID) && !String.IsNullOrWhiteSpace(clientSecret))
                        {
                            OAuth2Client.Instance.RegisterProvider(type, clientID, clientSecret, scope);
                        }
                    }
                    catch
                    {
                        //Not adding
                    }
                }
            }
        }
    }
}
