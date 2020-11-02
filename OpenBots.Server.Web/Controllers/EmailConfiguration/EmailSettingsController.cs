using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller for email settings
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailSettingsController : EntityController<EmailSettings>
    {
        /// <summary>
        /// EmailSettingsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public EmailSettingsController(
            IEmailSettingsRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
        }

        /// <summary>
        /// Provides all email settings
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of email settings</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of email settings</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<EmailSettings>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<EmailSettings> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Gets count of email settings in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all email settings</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of email settings</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Get email setting by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email setting exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email setting id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email setting exists for the given email setting id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email setting details</returns>
        [HttpGet("{id}", Name = "GetEmailSettings")]
        [ProducesResponseType(typeof(PaginatedList<EmailSettings>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                return await base.GetEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new email setting to the existing email settings
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">OK, new email setting created and returned</response>
        /// <response code="400">Bad request, when the email setting value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns> Newly created unique email settings </returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailSettings), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] EmailSettings request)
        {
            try
            {
                var emailAccount = repository.Find(null)?.Items?.FirstOrDefault();
                if (emailAccount != null && emailAccount.IsDeleted == false)
                {
                    ModelState.AddModelError("Email Settings", "Email settings already exist.  Please update existing settings.");
                    return BadRequest(ModelState);
                }

                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates email settings
        /// </summary>
        /// <remarks>
        /// Provides an action to update email settings, when email setting id and the new details of email setting are given
        /// </remarks>
        /// <param name="id">Email setting id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Email setting details to be updated</param>
        /// <response code="200">Ok, if the email setting details for the given email setting id has been updated.</response>
        /// <response code="400">Bad request, if the email setting id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailSettings), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] EmailSettings request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingEmailSettings = repository.GetOne(entityId);
                if (existingEmailSettings == null) return NotFound();

                existingEmailSettings.OrganizationId = request.OrganizationId;
                existingEmailSettings.IsEmailDisabled = request.IsEmailDisabled;
                existingEmailSettings.AddToAddress = request.AddToAddress;
                existingEmailSettings.AddCCAddress = request.AddCCAddress;
                existingEmailSettings.AddBCCAddress = request.AddBCCAddress;
                existingEmailSettings.AllowedDomains = request.AllowedDomains;
                existingEmailSettings.BlockedDomains = request.BlockedDomains;
                existingEmailSettings.SubjectAddPrefix = request.SubjectAddPrefix;
                existingEmailSettings.SubjectAddSuffix = request.SubjectAddSuffix;
                existingEmailSettings.BodyAddPrefix = request.BodyAddPrefix;
                existingEmailSettings.BodyAddSuffix = request.BodyAddSuffix;

                return await base.PutEntity(id, existingEmailSettings);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Email Account", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of email settings
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, If update of email settings is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>200 Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailSettings), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<EmailSettings> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete email settings with a specified id from email settings data table.
        /// </summary>
        /// <param name="id">Email setting id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when email setting is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email setting id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }
    }
}
