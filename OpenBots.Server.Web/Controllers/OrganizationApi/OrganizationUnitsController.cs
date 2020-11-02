using System;
using System.Linq;
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
    /// Controller for Organization Units
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationUnitsController : EntityController<OrganizationUnit>
    {
        IMembershipManager membershipManager;
        
        /// <summary>
        /// OrganizationUnitsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public OrganizationUnitsController(
            IOrganizationUnitRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.membershipManager = membershipManager;
            this.membershipManager.SetContext(base.SecurityContext);
        }

        /// <summary>
        /// Gets allorganization units details (departments) that are part of an organization.
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, paginated list of the organization units belonging to a particular organization</response>
        /// <response code="400">Bad request, if the organization id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Paginated list of all the organization units for a particulatular organization</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<OrganizationUnit>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<OrganizationUnit> Get(
            [FromRoute] string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            var departments = membershipManager.GetDepartments(organizationId);
            return departments;
        }

        /// <summary>
        /// Provides the organization unit details for a particular id
        /// </summary>
        /// <param name="id">Organization unit identifier</param>
        /// <response code="200">Ok, if organization unit detail is available with the given id></response>
        /// <response code="400">BadRequest, if organization unit id is not provided or improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user.</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Ok response with organization unit details</returns>
        [HttpGet("{id}", Name = "GetOrganizationUnit")]
        [ProducesResponseType(typeof(OrganizationUnit), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id)
        {
            var departments = await base.GetEntity(id);
            return departments;
        }

        /// <summary>
        /// Adds an organization unit to an organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="value">Value of organization unit to be added</param>
        /// <response code="200">Ok, if the unit has been added successfully to an organization</response>
        /// <response code="400">Bad request, if the organization id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error </response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered</response> 
        /// <returns>Ok response with organization unit details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrganizationUnit), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public  async Task<IActionResult> Post(string organizationId, [FromBody] OrganizationUnit value)
        {
            if (value == null && string.IsNullOrEmpty(organizationId))
            {
                ModelState.AddModelError("Add", "Organization Id not passed");
                return BadRequest(ModelState);
            }

            Guid entityId = new Guid(organizationId);

            if (value == null || value.OrganizationId == null || !value.OrganizationId.HasValue || entityId != value.OrganizationId.Value)
            {
                ModelState.AddModelError("Update", "Organization IDs don't match");
                return BadRequest(ModelState);
            }

            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Add", "Add department failed, administrator of an organization can only add departments");
                return BadRequest(ModelState);
            }

            var orgUnit = repository.Find(null, d => d.Name.ToLower(null) == value.Name.ToLower(null) && d.OrganizationId == value.OrganizationId)?.Items?.FirstOrDefault();
            if (orgUnit != null)
            {
                ModelState.AddModelError("Add", "Department Name already exists, cannot add duplicate");
                return BadRequest(ModelState);
            }

            value.OrganizationId = new Guid(organizationId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Updates a member of an organization unit
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Organization unit identifier</param>
        /// <param name="value">Value of the organization unit to be updated</param>
        /// <response code="200">Ok,if the value has been updated for the organization unit.</response>
        /// <response code="400">Bad request, if the organization id or unit id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error.</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Ok response with updated organization unit details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] OrganizationUnit value)
        {
            if (value == null && string.IsNullOrEmpty(organizationId))
            {
                ModelState.AddModelError("Update", "Organization details not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(organizationId);

            if (value == null || value.OrganizationId == null || !value.OrganizationId.HasValue || entityId != value.OrganizationId.Value)
            {
                ModelState.AddModelError("Update", "Organization IDs don't match");
                return BadRequest(ModelState);
            }

            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Update", "Update department failed, administrator of an organization can only update departments");
                return BadRequest(ModelState);
            }

            value.OrganizationId = new Guid(organizationId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Deletes an organization unit 
        /// </summary>
        /// <param name="id">Organization unit identifier</param>
        /// <response code="200">Ok, if the organization unit with the given id has been deleted for an Organization</response>
        /// <response code="400">Bad request, if the id is not provided or not in proper Guid format.</response>
        /// <response code="403">Unauthorized access, if the user doesn't have permission to delete the organization unit</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public  async Task<IActionResult> Delete(string organizationId, string id)
        {
            if (string.IsNullOrEmpty(organizationId) || string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "Organization Id / Department Id not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);
            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Delete", "Delete department failed, administrator of an organization can only delete departments");
                return BadRequest(ModelState);
            }

            var orgUnit = repository.Find(null, d => d.Id == entityId && d.CanDelete == false)?.Items?.FirstOrDefault();
            if (orgUnit != null)
            {
                ModelState.AddModelError("Delete", "Department cannot be deleted ");
                return BadRequest(ModelState);
            }
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates the partial details of an organization unit
        /// </summary>
        /// <param name="id">Organization unit identifier</param>
        /// <param name="value">Details of the organization unit to be updated</param>
        /// <response code="200">Ok, if update of organization unit is successful. </response>
        /// <response code="400">Bad request, if the id is null or ids dont match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial organization unit details have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string organizationId, string id,[FromBody] JsonPatchDocument<OrganizationUnit> value)
        {
            if (value == null || string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "Organization Department Id not passed");
                return BadRequest(ModelState);
            }

            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Update", "Update failed, administrator of an organization can only update");
                return BadRequest(ModelState);
            }
            return await base.PatchEntity(id, value);
        }
    }
}