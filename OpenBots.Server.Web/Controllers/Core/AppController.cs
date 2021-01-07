using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for application versioning
    /// </summary>
    [ApiVersionNeutral]
    [Route("Application")]
    [ApiController]
    [AllowAnonymous]
    public class AppController : EntityController<ApplicationVersion>
    {
        /// <summary>
        /// AppController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public AppController(
            IApplicationVersionRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        { }

        protected static string APPNAME = "OpenBots.Server";

        /// <summary>
        /// Application version
        /// </summary>
        /// <param name="application"></param>
        /// <response code="200">Ok, application version details</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with application version details</returns>
        [HttpGet("{application}/Version")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Version(string application = "OpenBots.Server")
        {
            try
            {
                var app = repository.Find(null, p => p.Name.Equals(application, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                if (app == null)
                {
                    ModelState.AddModelError("", "No application found");
                    return BadRequest(ModelState);
                }
                var appVersion = string.Format("{0}.{1}.{2}", app.Major, app.Minor, app.Patch);
                return Ok(appVersion);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Patch release
        /// </summary>
        /// <param name="application"></param>
        /// <param name="key"></param>
        /// <response code="200">Ok, updated application version details</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with updated application version details</returns>
        [HttpPut("{application}/Version/Patch/Release")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PatchRelease(string application= "OpenBots.Server", [FromQuery()] string key = "")
        {
            if (string.IsNullOrEmpty(key) || !key.Equals("TUREACBITR", StringComparison.InvariantCultureIgnoreCase))
                return Unauthorized();
            try
            {
                var app = repository.Find(null, p => p.Name.Equals(application, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                if (app == null)
                    return NotFound();

                app.Patch = app.Patch + 1;
                repository.Update(app);
                var appVersion = string.Format("{0}.{1}.{2}", app.Major, app.Minor, app.Patch);
                return Ok(appVersion);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Minor version release
        /// </summary>
        /// <param name="application"></param>
        /// <param name="key"></param>
        /// <response code="200">Ok, updated application version details</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with updated application version details</returns>
        [HttpPut("{application}/Version/Minor/Release")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> MinorRelease(string application = "OpenBots.Server", [FromQuery()] string key = "")
        {
            if (string.IsNullOrEmpty(key) || !key.Equals("TUREACBITR"))
                return Unauthorized();
            try
            {
                var app = repository.Find(null, p => p.Name.Equals(application, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                if (app == null)
                    return NotFound();

                app.Minor = app.Minor + 1;
                repository.Update(app);
                var appVersion = string.Format("{0}.{1}.{2}", app.Major, app.Minor, app.Patch);
                return Ok(appVersion);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}