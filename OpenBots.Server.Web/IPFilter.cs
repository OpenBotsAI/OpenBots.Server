using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Business;
using System.Net;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    public class IPFilter
    {
        private readonly RequestDelegate _next;

        public IPFilter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context,
            IIPFencingManager iPFencingManager)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            bool isAllowedRequest = iPFencingManager.IsRequestAllowed(ipAddress);

            if (!isAllowedRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.WriteAsync("Current IP Address is blocked.");
                return;
            }
            await _next.Invoke(context);
        }
    }

    //extension method used to add the middleware to the HTTP request pipeline
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPFilter>();
        }
    }
}
