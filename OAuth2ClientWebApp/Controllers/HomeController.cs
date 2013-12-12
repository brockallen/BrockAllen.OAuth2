using BrockAllen.OAuth2;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace OAuth2ClientWebApp.Controllers
{
    public class HomeController : Controller
    {
        static HomeController()
        {
            RegisterOAuth2Clients();
        }

        static void RegisterOAuth2Clients()
        {
            // these can be registered here or in web.config
            OAuth2Client.Instance.RegisterProvider(
                ProviderType.Google,
                "421418234584-3n8ub7gn7gt0naghh6sqeu7l7l45te1c.apps.googleusercontent.com",
                "KDJt_7Rm6Or2pJulBdy0gvpx");

            OAuth2Client.Instance.RegisterProvider(
                ProviderType.Facebook,
                "195156077252380",
                "39b565fd85265c56010555f670573e28");

            OAuth2Client.Instance.RegisterProvider(
                ProviderType.Live,
                "00000000400DF045",
                "4L08bE3WM8Ra4rRNMv3N--un5YOBr4gx");

            OAuth2Client.Instance.RegisterProvider(
                ProviderType.LinkedIn,
                "bcfpbssjiwo7",
                "k75VoLERs3isUxAL");
        }

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Login(string type)
        {
            // 1st param is which OAuth2 provider to use
            // 2nd param is what URL to send the user once all the login magic is done
            if (Config.UseCustomCallback)
            {
                return new OAuth2ActionResult(type);
            }
            else
            {
                return new OAuth2ActionResult(type, Url.Action("Index"));
            }
        }

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

            var token = new SessionSecurityToken(cp);
            sam.WriteSessionTokenToCookie(token);

            return RedirectToAction("Index");
        }
    }
}
