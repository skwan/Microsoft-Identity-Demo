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

            string[] scopes = { "https://graph.microsoft.com/people.read" };

            AuthenticationResult result = await client.AcquireTokenSilentAsync(scopes);

            string graphRequest = "https://graph.microsoft.com/beta/me/people";

            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphRequest);
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
    }
}