using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Security.Claims;

namespace Microsoft_Identity_Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            ViewBag.Message = "Display the current user's claims.";
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                ViewBag.Claims = ClaimsPrincipal.Current.Claims;
            }
            return View();
        }

        [Authorize]
        public ActionResult Calendar()
        {
            return View();
        }
    }
}