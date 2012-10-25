using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    public class OAuth2ActionResult : System.Web.Mvc.ActionResult
    {
        ProviderType type;
        string returnUrl;
        public OAuth2ActionResult(ProviderType type, string returnUrl)
        {
            this.type = type;
            this.returnUrl = returnUrl;
        }

        public override void ExecuteResult(System.Web.Mvc.ControllerContext context)
        {
            OAuth2Client.RedirectToAuthorizationProvider(type, context.HttpContext, returnUrl);
        }
    }
}
