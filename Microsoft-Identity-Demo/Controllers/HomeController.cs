using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.Identity.Client;
using Microsoft_Identity_Demo.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

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
            if (ClaimsPrincipal.Current.Identity.IsAuthenticated)
            {
                ViewBag.Claims = ClaimsPrincipal.Current.Claims;
            }
            return View();
        }

        [Authorize]
        public async Task<ActionResult> Calendar()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

            MSALSessionCache tokenCache = new MSALSessionCache(signedInUserID, this.HttpContext);

            ConfidentialClientApplication client = new ConfidentialClientApplication(
                clientId: "82c89714-f7a5-4088-9bc0-cd66410889ae",
                clientCredential: new ClientCredential("O4FycGEof1N00sZzzKWcYCp"),
                redirectUri: "https://localhost:44300/",
                userTokenCache: tokenCache
                );

            string[] scopes = { "https://graph.microsoft.com/calendars.read" };

            AuthenticationResult result = await client.AcquireTokenSilentAsync(scopes);

            DateTime yesterdayDateTime = DateTime.Now.AddDays(-1);
            DateTime tomorrowDateTime = DateTime.Now.AddDays(1);
            string yesterdayDateTimeString = yesterdayDateTime.ToUniversalTime().ToString("u");
            string tomorrowDateTimeString = tomorrowDateTime.ToUniversalTime().ToString("u");

            string graphRequest = "https://graph.microsoft.com/v1.0/me/calendar/calendarview?" + "startDateTime=" + yesterdayDateTimeString + "&" + "endDateTime=" + tomorrowDateTimeString;

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphRequest);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                String responseString = await response.Content.ReadAsStringAsync();
                JObject mailResponse = JObject.Parse(responseString);
                JArray messages = mailResponse["value"] as JArray;
                ViewBag.Events = messages;
                return View();
            }

            return View();
        }
        [Authorize]
        public async Task<ActionResult> RelatedPeople()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

            MSALSessionCache tokenCache = new MSALSessionCache(signedInUserID, this.HttpContext);

            ConfidentialClientApplication client = new ConfidentialClientApplication(
                clientId: "82c89714-f7a5-4088-9bc0-cd66410889ae",
                clientCredential: new ClientCredential("O4FycGEof1N00sZzzKWcYCp"),
                redirectUri: "https://localhost:44300/",
                userTokenCache: tokenCache
                );

            AuthenticationResult result = null;
            string[] scopes = { "https://graph.microsoft.com/people.read" };

            try
            {
                result = await client.AcquireTokenSilentAsync(scopes);
            }
            catch (Exception e)
            {
                ViewBag.NeedConsent = true;
                return View();
            }

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/beta/me/people");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                String responseString = await response.Content.ReadAsStringAsync();
                JObject relatedPeopleResponse = JObject.Parse(responseString);
                JArray people = relatedPeopleResponse["value"] as JArray;
                ViewBag.People = people;
                return View();
            }
            
            return View();
        }
        public void GetConsent ()
        {
            Dictionary<string, string> scopeDictionary = new Dictionary<string, string>() { { "scope", "https://graph.microsoft.com/people.read" } };
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties(scopeDictionary) { RedirectUri = "/Home/RelatedPeople" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }
        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(AuthenticationProperties properties, string authenticationType)
            {
                Properties = properties;
                Type = authenticationType;
            }

            public AuthenticationProperties Properties { get; set; }
            public string Type { get; set; }
            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.GetOwinContext().Authentication.Challenge(Properties, Type);
            }
        }
    }
}