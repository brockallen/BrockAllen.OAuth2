using BrockAllen.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace OAuth2ClientWebApp.Controllers
{
    public class HomeController : Controller
    {
        static HomeController()
        {
            OAuth2Client.RegisterProvider(
                ProviderType.Google,
                "421418234584-3n8ub7gn7gt0naghh6sqeu7l7l45te1c.apps.googleusercontent.com",
                "KDJt_7Rm6Or2pJulBdy0gvpx");
            OAuth2Client.RegisterProvider(
                ProviderType.Facebook, 
                "195156077252380",
                "39b565fd85265c56010555f670573e28");
            OAuth2Client.RegisterProvider(
                ProviderType.Live, 
                "00000000400DF045",
                "4L08bE3WM8Ra4rRNMv3N--un5YOBr4gx");
        }

        public ActionResult Index()
        {
            var url = Url.Action("Callback", "OAuthController", new { area = "OAuth2Client" });
            return View();
        }
        
        public ActionResult Login(ProviderType name)
        {
            return new OAuth2ActionResult(name, Url.Action("Index"));
        }
    }
}
