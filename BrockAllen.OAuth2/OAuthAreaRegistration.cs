using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }
    }
}
