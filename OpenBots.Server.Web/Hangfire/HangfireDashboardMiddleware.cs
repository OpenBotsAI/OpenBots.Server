using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Hangfire
{
    public class HangfireDashboardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JobStorage _storage;
        private readonly DashboardOptions _options;
        private readonly RouteCollection _routes;

        public HangfireDashboardMiddleware(RequestDelegate next, JobStorage storage, DashboardOptions options, RouteCollection routes)
        {
            _next = next;
            _storage = storage;
            _options = options;
            _routes = routes;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var findResult = _routes.FindDispatcher(httpContext.Request.Path.Value);
            if (findResult == null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            //Attempt to authenticate against default auth scheme (this will attempt to authenticate using data in request, but doesn't send challenge)
            var result = await httpContext.AuthenticateAsync();
            if (!result.Succeeded)
            {
                //Request was not authenticated, send challenge and do not continue processing this request
                await httpContext.ChallengeAsync();
                return;
            }

            var aspNetCoreDashboardContext = new AspNetCoreDashboardContext(_storage, _options, httpContext);

            foreach (var filter in _options.Authorization)
            {
                if (filter.Authorize(aspNetCoreDashboardContext) == false)
                {
                    var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated;
                    httpContext.Response.StatusCode = isAuthenticated == true ? (int)HttpStatusCode.Forbidden : (int)HttpStatusCode.Unauthorized;
                    return;
                }
            }
            aspNetCoreDashboardContext.UriMatch = findResult.Item2;
            await findResult.Item1.Dispatch(aspNetCoreDashboardContext);
        }
    }

    public class CustomHangfireDashboardMiddleware
    {
        private readonly RequestDelegate _nextRequestDelegate;
        private readonly JobStorage _jobStorage;
        private readonly DashboardOptions _dashboardOptions;
        private readonly RouteCollection _routeCollection;

        public CustomHangfireDashboardMiddleware(
            RequestDelegate nextRequestDelegate,
            JobStorage storage,
            DashboardOptions options,
            RouteCollection routes)
        {
            _nextRequestDelegate = nextRequestDelegate;
            _jobStorage = storage;
            _dashboardOptions = options;
            _routeCollection = routes;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var aspNetCoreDashboardContext = new AspNetCoreDashboardContext(_jobStorage, _dashboardOptions, httpContext);
            var findResult = _routeCollection.FindDispatcher(httpContext.Request.Path.Value);
            if (findResult == null)
            {
                await _nextRequestDelegate.Invoke(httpContext);
                return;
            }

            //Attempt to authenticate against Cookies scheme.
            //This will attempt to authenticate using data in request, but doesn't send challenge.
            var result = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return;
            }

            if (_dashboardOptions.Authorization.Any(filter => filter.Authorize(aspNetCoreDashboardContext) == false))
            {
                var isAuthenticated = result.Principal?.Identity?.IsAuthenticated ?? false;
                if (isAuthenticated == false)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                return;
            }

            aspNetCoreDashboardContext.UriMatch = findResult.Item2;
            await findResult.Item1.Dispatch(aspNetCoreDashboardContext);
        }
    }
}
