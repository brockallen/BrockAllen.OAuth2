using BrockAllen.OAuth2;
using System;
using System.Collections.Generic;
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
        }

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Login(ProviderType type)
        {
            // 1st param is which OAuth2 provider to use
            // 2nd param is what URL to send the user once all the login magic is done
            return new OAuth2ActionResult(type, Url.Action("Index"));
        }
    }
}
