using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for organizations
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationsController : EntityController<Organization>
    {
        IMembershipManager membershipManager;
        IOrganizationManager organizationManager;

        /// <summary>
        /// OrganizationsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="organizationManager"></param>
        /// <param name="httpContextAccessor"></param>
        public OrganizationsController(
            IOrganizationRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IOrganizationManager organizationManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.membershipManager = membershipManager;
            this.organizationManager = organizationManager;
            this.membershipManager.SetContext(base.SecurityContext);
            this.organizationManager.SetContext(base.SecurityContext);

        }

        /// <summary>
        /// Provides a list of all organizations
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all organizations</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all organizations</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Organization>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<Organization> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides count of organizations
        /// </summary>
        /// <response code="200">Ok, an organization count</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Organizations count</returns>
        [AllowAnonymous]
        [HttpGet("TotalOrganizationCount")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public int? Get()
        {
            return base.Count();
        }

        /// <summary>
        /// Provides organization details for a particular organization id
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <response code="200">Ok, if an organization exists with the given id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if organization id is not in proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no organization exists for the given organization id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Organization details for the given id</returns>
        [HttpGet("{id}", Name = "GetOrganization")]
        [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
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
                var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(id), SecurityContext.PersonId)?.Items?.FirstOrDefault();

                if (orgmem == null)
                {
                    return Ok();
                }

                var result = base.GetEntity(id).Result;
                if (result.GetType() == typeof(StatusCodeResult)) { return result; }
                var organization = ((OkObjectResult)result).Value as Organization;
                //Need to get organization member using logged in person Id 
                
                if (orgmem != null)
                    organization.Members.Add(orgmem);

                return Ok(organization);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new organization to the existing organizations
        /// </summary>
        /// <remarks>
        /// Adds the organization with unique organization id to the existing organizations
        /// </remarks>
        /// <param name="value"></param>
        /// <response code="200">Ok, new organization created and returned</response>
        /// <response code="400">Bad  request, when the organization value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique organization details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] Organization value)
        {
            if (value == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }            

            Guid entityId = Guid.NewGuid();
            if (value.Id == null || value.Id.HasValue || value.Id.Equals(Guid.Empty))
                value.Id = entityId;

            try
            {
                if (SecurityContext == null)
                {
                    ModelState.AddModelError("Add", "Create organization failed. Logged in user no longer exists in system.");
                    return BadRequest(ModelState);
                }

                //Same name organization check
                var existingOrg = repository.Find(null, o => o.Name.Trim().ToLower() == value.Name.Trim().ToLower()).Items?.FirstOrDefault();
                if (existingOrg != null)
                {
                    ModelState.AddModelError("Add", "Organization name already exists. cannot add duplicate.");
                    return BadRequest(ModelState);
                }

                var newOrganization = organizationManager.AddNewOrganization(value);
                return Ok(newOrganization);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an organization 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an organization, when organization id and the new details of organization are given
        /// </remarks>
        /// <param name="id">organization id, produces bad request if id is null or ids don't match</param>
        /// <param name="value">Organization details to be updated</param>
        /// <response code="200">Ok if the organization details for the given organization id have been updated</response>
        /// <response code="400">Bad request, if the organization id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] Organization value)
        {
            if (value == null && string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "Organization details not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);

            if (value == null || value.Id == null || !value.Id.HasValue || entityId != value.Id.Value)
            {
                ModelState.AddModelError("Update", "Organization IDs don't match");
                return BadRequest(ModelState);
            }

            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(id), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Update", "Update failed, administrator of an organization can only update");
                return BadRequest(ModelState);
            }

            //Same name organization check
            var existingOrg = repository.Find(null, o => o.Name.Trim().ToLower() == value.Name.Trim().ToLower() && o.Id != entityId).Items?.FirstOrDefault();
            if (existingOrg != null)
            {
                ModelState.AddModelError("Update", "Organization name already exists. cannot add duplicate.");
                return BadRequest(ModelState);
            }

            var org = repository.GetOne(Guid.Parse(id));
            org.Name = !string.IsNullOrEmpty(value.Name) && !org.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase) ? value.Name : org.Name;
           
            return await base.PutEntity(id, org).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an organization with a specified id from the organizations.
        /// </summary>
        /// <param name="id">Organization id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when organization is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if organization id is null or empty Guid</response>
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
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Delete", "Organization Id not passed");
                return BadRequest(ModelState);
            }

            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(id), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Update", "Delete failed, administrator of an organization can only delete");
                return BadRequest(ModelState);
            }

            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of an organization
        /// </summary>
        /// <param name="id">Organization identifier</param>
        /// <param name="value">Value of the organization to be updated</param>
        /// <response code="200">Ok, if update of Organization is successful</response>
        /// <response code="400">BadRequest, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial organization values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Organization> value)
        {
            if (value == null && string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "Organization details not passed");
                return BadRequest(ModelState);
            }
            
            var orgmem = membershipManager.GetOrganizationMember(Guid.Parse(id), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Update", "Update failed, administrator of an organization can only update");
                return BadRequest(ModelState);
            }

            return await base.PatchEntity(id, value);
        }
    }
}
