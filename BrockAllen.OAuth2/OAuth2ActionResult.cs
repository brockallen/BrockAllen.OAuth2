using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.OAuth2
{
    public class OAuth2ActionResult : System.Web.Mvc.ActionResult
    {
        string type;
        string returnUrl;
        OAuth2Client client;

        public OAuth2ActionResult(string type)
            : this(OAuth2Client.Instance, type, null)
        {
        }
        
        public OAuth2ActionResult(string type, string returnUrl)
            : this(OAuth2Client.Instance, type, returnUrl)
        {
        }

        public OAuth2ActionResult(OAuth2Client client, string type, string returnUrl)
        {
            this.client = client;
            this.type = type;
            this.returnUrl = returnUrl;
        }

        public override void ExecuteResult(System.Web.Mvc.ControllerContext context)
        {
            client.RedirectToAuthorizationProvider(type, returnUrl);
        }
    }
}
