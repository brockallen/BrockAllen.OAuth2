using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

                var settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
                var providers = from v in
                                    (from string q in settings.Keys
                                     where q.StartsWith("oauth2:")
                                     select new { key = q.Split(':'), value = settings[q] })
                                group v by v.key[1];
                foreach (var provider in providers)
                {
                    try
                    {
                        ProviderType type = provider.Key == "facebook" ? ProviderType.Facebook :
                                            provider.Key == "google" ? ProviderType.Google :
                                            ProviderType.Live;

                        var scope = provider.FirstOrDefault(k => k.key[2].ToLower() == "scope");

                        OAuth2Client.Instance.RegisterProvider(type,
                                    provider.First(k => k.key[2].ToLower() == "clientid").value,
                                    provider.First(k => k.key[2].ToLower() == "clientsecret").value,
                                    scope == null || String.IsNullOrEmpty(scope.value) ? null : scope.value);
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
