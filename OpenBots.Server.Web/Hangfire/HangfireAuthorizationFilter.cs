using Hangfire.Dashboard;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Hangfire
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity.IsAuthenticated; //allow access to any user
        }
    }

    public class HangfireAuthorizationFilterByQuerry : IDashboardAuthorizationFilter
    {
        private static readonly string HangFireCookieName = "HangfireCookie";
        private static readonly int CookieExpirationMinutes = 5;
        private TokenValidationParameters tokenValidationParameters;
        readonly IConfiguration configuration;

        public HangfireAuthorizationFilterByQuerry(TokenValidationParameters tokenValidationParameters, IConfiguration configuration)
        {
            this.tokenValidationParameters = tokenValidationParameters;
            this.configuration = configuration;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            Uri baseUrl = new Uri(configuration["WebAppUrl:Url"]);
            string url = new Uri(baseUrl, "/pages/refreshhangfire").ToString();

            try
            {
                var access_token = String.Empty;
                var setCookie = false;

                //Try to get token from query string
                if (httpContext.Request.Query.ContainsKey("access_token"))
                {
                    access_token = httpContext.Request.Query["access_token"].FirstOrDefault();
                    setCookie = true;
                }
                else if (httpContext.Request.Cookies.ContainsKey(HangFireCookieName))
                {
                    access_token = httpContext.Request.Cookies[HangFireCookieName];
                }

                if (String.IsNullOrEmpty(access_token))
                {
                    httpContext.Response.Redirect(url, false);
                    return true;
                }

                SecurityToken validatedToken = null;
                JwtSecurityTokenHandler hand = new JwtSecurityTokenHandler();
                var claims = hand.ValidateToken(access_token, this.tokenValidationParameters, out validatedToken);

                if (setCookie && claims?.Identity?.IsAuthenticated == true)
                {
                    if (httpContext.Request.Cookies.ContainsKey(HangFireCookieName))
                        httpContext.Response.Cookies.Delete(HangFireCookieName);

                    httpContext.Response.Cookies.Append(HangFireCookieName,
                    access_token,
                    new CookieOptions()
                    {
                        Expires = DateTime.Now.AddMinutes(CookieExpirationMinutes)
                    });
                }

                if (claims?.Identity?.IsAuthenticated == false)
                    httpContext.Response.Redirect(url, false);

                return true;
            }
            catch (Exception ex)
            {
                httpContext.Response.Redirect(url, false);
                return true;
            }
        }
    }
}

