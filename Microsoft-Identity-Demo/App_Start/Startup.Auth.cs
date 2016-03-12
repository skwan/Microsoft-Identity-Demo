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
                        // The `Authority` represents the v2.0 endpoint - https://login.microsoftonline.com/common/v2.0 
                        // The `Scope` describes the permissions that your app will need.  See https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes/
                        // In a real application you could use issuer validation for additional checks, like making sure the user's organization has signed up for your app, for instance.

                        ClientId = "82c89714-f7a5-4088-9bc0-cd66410889ae",
                        Authority = "https://login.microsoftonline.com/common/v2.0",
                        RedirectUri = "https://localhost:44300/",
                        Scope = "openid email profile",
                        ResponseType = "id_token",
                        PostLogoutRedirectUri = "https://localhost:44300/",
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                        },
                        Notifications = new OpenIdConnectAuthenticationNotifications
                        {
                            AuthenticationFailed = OnAuthenticationFailed,
                        }
                    });
        }
		private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}