using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BrockAllen.OAuth2
{
    public class OAuth2CallbackController : Controller
    {
        public async Task<ActionResult> Callback()
        {
            var result = await OAuth2Client.Instance.ProcessCallbackAsync();
            if (result.Error != null)
            {
                return Content(result.Error + "<br>" + result.ErrorDetails);
            }

            var sam = FederatedAuthentication.SessionAuthenticationModule;
            if (sam == null) throw new Exception("SessionAuthenticationModule not registered.");

            var cp = new ClaimsPrincipal(new ClaimsIdentity(result.Claims, "OAuth"));
            
            var id = cp.Identities.First();
            var authInstantClaim = new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString("s"));
            id.AddClaim(authInstantClaim);
            var idpClaim = new Claim(Constants.ClaimTypes.IdentityProvider, result.ProviderName);
            id.AddClaim(idpClaim);

            var transformer = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthenticationManager;
            if (transformer != null)
            {
                cp = transformer.Authenticate(String.Empty, cp);
            }
            var token = new SessionSecurityToken(cp);
            sam.WriteSessionTokenToCookie(token);

            if (!String.IsNullOrWhiteSpace(result.ReturnUrl))
            {
                return Redirect(result.ReturnUrl);
            }
            else
            {
                return Redirect("~");
            }
        }
    }
}
