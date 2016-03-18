using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using Microsoft.Identity.Client;
using Microsoft_Identity_Demo.Models;
using System.IdentityModel.Claims;

namespace Microsoft_Identity_Demo
{
	public partial class Startup
	{
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                    new OpenIdConnectAuthenticationOptions
                    {
                        //
                        // Initialize the OpenID Connect middleware.
                        //
                        Authority       = "https://login.microsoftonline.com/common/v2.0",
                        ClientId        = "82c89714-f7a5-4088-9bc0-cd66410889ae",
                        Scope           = "openid profile offline_access",
                        ResponseType    = "id_token code",
                        RedirectUri     = "https://localhost:44300/",

                        PostLogoutRedirectUri = "https://localhost:44300/",

                        /* IMPORTANT:  You must include application logic to decide which tenants are
                                       subscribers to your service and which are not, and validate
                                       the issuer value in the token.  Disabling issuer validation
                                       entirely as in this sample is only correct if you blanket accept
                                       sign ins from every Microsoft identity. */
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                        },

                        Notifications = new OpenIdConnectAuthenticationNotifications
                        {
                            AuthenticationFailed = OnAuthenticationFailed,
                            AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                        }
                    });
        }
        private async Task<object> OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
        {
            MSALSessionCache tokenCache = new MSALSessionCache(
                notification.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value,
                notification.OwinContext.Environment["System.Web.HttpContextBase"] as HttpContextBase
                );

            ConfidentialClientApplication client = new ConfidentialClientApplication(
                clientId:           "82c89714-f7a5-4088-9bc0-cd66410889ae",
                clientCredential:   new ClientCredential("O4FycGEof1N00sZzzKWcYCp"),
                redirectUri:        "https://localhost:44300/",
                userTokenCache:     tokenCache
                );

            string[] scopes = { "https://graph.microsoft.com/calendars.read", "https://graph.microsoft.com/people.read" };

            try
            {
                AuthenticationResult result = await client.AcquireTokenByAuthorizationCodeAsync(
                    scope:              scopes,
                    authorizationCode:  notification.Code
                    );
            }
            catch (Exception e)
            {
                // Nothing here for now.
            }

            return Task.FromResult<object>(0);
        }
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}