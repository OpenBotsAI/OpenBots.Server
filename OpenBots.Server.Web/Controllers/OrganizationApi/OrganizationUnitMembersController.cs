using System;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Business;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for Organization Unit Members
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationUnitMembersController : EntityController<OrganizationUnitMember>
    {
        /// <summary>
        /// OrganizationUnitMembersController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public OrganizationUnitMembersController(
            IOrganizationUnitMemberRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {
        }

        /// <summary>
        /// Gets all the members of an organization unit (department)
        /// </summary>
        /// <param name="organizationId">organization identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, a paginated list of the members of an organization unit</response>
        /// <response code="400">Bad request, if the Organization unit id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Paginated list of all the members of the organization unit</returns>
        [HttpGet]
        [ProducesResponseType(typeof(OrganizationUnitMember), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<OrganizationUnitMember> Get(
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
        /// Gets the organization unit member for a particular organization.
        /// </summary>
        /// <param name="id">Organization unit member id </param>
        /// <response code="200">Ok, if organization unit member is available within the organization unit></response>
        /// <response code="400">Bad request, if the id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Ok response with organization unit member details</returns>
        [HttpGet("{id}", Name = "GetOrganizationUnitMember")]
        [ProducesResponseType(typeof(OrganizationUnitMember), StatusCodes.Status200OK)]
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
        /// Adds a organization unit member to an organization unit
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="value">Details of the organization unit member</param>
        /// <response code="200">Ok, if the member has been added successfully to an organization unit</response>
        /// <response code="400">Bad request, if the organization id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered</response> 
        /// <returns>Ok response with organization unit member details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrganizationUnitMember), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string organizationId, [FromBody] OrganizationUnitMember value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Updates a member of an organization unit
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Member identifier</param>
        /// <param name="value">Member value to be updated</param>
        /// <response code="200">Ok, if the value has been updated for the organization member</response>
        /// <response code="400">Bad request, if the organization id or member id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with the updated organization member details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] OrganizationUnitMember value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Delete member from an organization unit
        /// </summary>
        /// <param name="id">Organization member identifier</param>
        /// <response code="200">Ok, if the organization member with the given id has been deleted for an organization unit</response>
        /// <response code="400">Bad request, if the id is not provided or not in proper Guid format</response>
        /// <response code="403">Unauthorized access, if the user doesn't have permission to delete the organization member</response>
        /// <response code="422">Unprocessable Entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates the partial details of the organization unit member
        /// </summary>
        /// <param name="id">Organization unit member identifier</param>
        /// <param name="value">Value of the organization unit member to be updated</param>
        /// <response code="200">Ok, if update of organization unit member is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial Organization values has been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,[FromBody] JsonPatchDocument<OrganizationUnitMember> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}