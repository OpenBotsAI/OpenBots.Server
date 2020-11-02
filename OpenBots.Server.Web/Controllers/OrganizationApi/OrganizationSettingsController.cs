using System;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Security;
using OpenBots.Server.Business;
using Microsoft.AspNetCore.Authorization;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for organization settings
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationSettingsController : EntityController<OrganizationSetting>
    {
        /// <summary>
        /// OrganizationSettingsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public OrganizationSettingsController(
            IOrganizationSettingRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
        }

        /// <summary>
        /// Gets all the organization settings for an organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, provides a paginated list of the organization settings</response>
        /// <response code="400">Bad request, if the organization id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Paginated list of all the organization settings for an organization</returns>
        [HttpGet]
        [ProducesResponseType(typeof(OrganizationSetting), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<OrganizationSetting> Get(
            [FromRoute] string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany(organizationId);
        }

        /// <summary>
        /// Gets a particular organization setting
        /// </summary>
        /// <param name="id">Organization setting identifier</param>
        /// <response code="200">Ok, provides organization setting details></response>
        /// <response code="400">Bad request, if the organization id or setting id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with organization setting details</returns>
        [HttpGet("{id}", Name = "GetOrganizationSetting")]
        [ProducesResponseType(typeof(OrganizationSetting), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id)
        {
            return await base.GetEntity(id);
        }

        /// <summary>
        /// Adds an organization setting for an organization id 
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="value">Details of the organization setting</param>
        /// <response code="200">Ok, if the new setting has been added successfully</response>
        /// <response code="400">Bad request, if the irganization id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error </response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered</response>
        /// <returns>Ok response with organization setting details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrganizationSetting), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string organizationId, [FromBody] OrganizationSetting value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Updates the organization setting for an organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Organization setting identifier </param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if the value has been updated for the organization setting for an organization</response>
        /// <response code="400">Bad request, if the organization id or setting id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with organization setting details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] OrganizationSetting value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Deletes setting with the specific id
        /// </summary>
        /// <param name="id">Organization setting identifier</param>
        /// <response code="200">Ok, if the setting has been soft deleted</response>
        /// <response code="400">Bad request, if the id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
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

        /// <summary>
        /// Partially updates the organization setting 
        /// </summary>
        /// <param name="id">Organization setting identifier</param>
        /// <param name="value">Values to be updated</param>
        /// <response code="200">Ok, if update of organization setting is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial organization setting values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<OrganizationSetting> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}