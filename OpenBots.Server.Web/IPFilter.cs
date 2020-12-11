using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
            IOrganizationManager organizationManager,
            IIPFencingManager iPFencingManager,
            IIPFencingRepository iPFencingRepository)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            bool isAllowedRequest = false;         
            isAllowedRequest = iPFencingManager.IsRequestAllowed(ipAddress);

            if (!isAllowedRequest)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
            await _next.Invoke(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPFilter>();
        }
    }
}
