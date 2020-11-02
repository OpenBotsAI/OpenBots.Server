using System;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for access requests
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class AccessRequestsController : EntityController<AccessRequest>
    {
        IMembershipManager _manager;
        IAccessRequestsManager accessRequestManager;

        /// <summary>
        /// AccessRequestsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="manager"></param>
        /// <param name="accessRequestManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public AccessRequestsController(
            IAccessRequestRepository repository,
            IMembershipManager manager,
            IAccessRequestsManager accessRequestManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, manager, configuration)
        {
            _manager = manager;
            this.accessRequestManager = accessRequestManager;

            _manager.SetContext(base.SecurityContext); ;
            this.accessRequestManager.SetContext(base.SecurityContext);

        }

        /// <summary>
        /// Provides all the access requests for the given organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, if all the access requests for the organization have been returned</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found, no access request exists for the given organization</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Paginated list of all access requests for the organization</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AccessRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public PaginatedList<AccessRequest> Get(
            [FromRoute] string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return accessRequestManager.GetAccessRequests(organizationId);//base.GetMany(organizationId);
        }

        /// <summary>
        /// Pending access requests
        /// </summary>
        /// <remarks>Provides the paginated pending access requests for the organization</remarks>
        /// <param name="organizationId">Organization identifier</param>
        /// <response code="200">Ok, paginated list of all access requests with id, name, and email of the user</response>
        /// <response code="400">Bad request, if organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>List of pending access requests with id, name,, and email of each user</returns>
        [HttpGet("Pending")]
        [ProducesResponseType(typeof(PaginatedList<PendingAccessRequest>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> GetPending(string organizationId)
        {
            Guid orgGuid = Guid.Parse(organizationId);
            try
            {
                var result = _manager.Pending(orgGuid);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Get the access request
        /// </summary>
        /// <param name="id">Access request identifier</param>
        /// <response code="200">Ok, if access request exists for the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if the id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found, access request with the particular id does not exist</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with access request details</returns>
        [HttpGet("{id}", Name = "GetAccessRequest")]
        [ProducesResponseType(typeof(AccessRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            return await base.GetEntity(id);
        }

        /// <summary>
        /// Adds a new access request to the organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="value">Access request details</param>
        /// <response code="200">Ok, if the access request has been created successfully</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, when access request with the particular id already exists</response>
        /// <response code="422">Unprocessable entity, validation error or cannot insert duplicate constraint</response>
        /// <returns>Ok response with newly created access request details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AccessRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Post(string organizationId, [FromBody] AccessRequest value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Update the access request 
        /// </summary>
        /// <remarks>Updates the access request with the particular id for the given organization</remarks>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Access request id</param>
        /// <param name="value">New value of the access request to be updated</param>
        /// <response code="200">Ok, if the update of the access request for the particular id has been successful</response>
        /// <response code="400">Bad request, if the id is not provided or Guid is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="404">Not found, if no access request exists for the given id</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable Entity, validation error</response>
        /// <returns>Ok response with the updated access request details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] AccessRequest value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Approves the specified acess request by an organization administrator.
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Access request identifier</param>
        /// <response code="200">Ok, if the access request is approved</response>
        /// <response code="400">Bad request,if the organization id or access request id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, if the user doesn't have permission to approve an access request</response>
        /// <response code="422">Unprocessable entity, validation error</response> 
        /// <returns>Ok response if the approval is successful</returns> 
        [HttpPut("{id}/Approve")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Approve(string organizationId, string id)
        {
            if ( string.IsNullOrEmpty(id) || string.IsNullOrEmpty(organizationId))
            {
                ModelState.AddModelError("Approve", "Access or Organization Id not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);

            var orgmem = _manager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Approve", "Approve failed, administrator of an organization can only Approve");
                return BadRequest(ModelState);
            }

            Guid approvalRequestGuid = Guid.Parse(id);
            try
            {
                _manager.ApproveAccessRequest(approvalRequestGuid, SecurityContext);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Rejects the specified access request by an organization administrator
        /// </summary>
        /// <param name="organizationId">Organization identifier.</param>
        /// <param name="id">Access request identifier</param>
        /// <response code="200">Ok, if the acccess request is rejected</response>
        /// <response code="400">Bad request, if the organization id or access request id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, if the user doesn't have permission to reject the access request</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response if the access is rejected</returns>
        [HttpPut("{id}/Reject")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Reject(string organizationId, string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(organizationId))
            {
                ModelState.AddModelError("Approve", "Access or Organization Id not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);

            var orgmem = _manager.GetOrganizationMember(Guid.Parse(organizationId), SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Approve", "Approve failed, administrator of an organization can only Approve");
                return BadRequest(ModelState);
            }

            Guid approvalRequestGuid = Guid.Parse(id);

            try
            {
                _manager.RejectAccessRequest(approvalRequestGuid, SecurityContext);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes access request 
        /// </summary>
        /// <param name="id">Access request identifier</param>
        /// <response code="200">Ok, if the access request with the given id has been soft deleted</response>
        /// <response code="400">Bad request, if the id is not provided or not a proper Guid</response>
        /// <response code="403">Unauthorized access, if the user doesn't have permission to soft delete the access request</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the access request with the given id has been deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }


        /// <summary>
        /// Updates partial details of an access request
        /// </summary>
        /// <param name="id">Access request identifier</param>
        /// <param name="value">Details of access request to be updated</param>
        /// <response code="200">Ok, if update of access request is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial access request details have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody]JsonPatchDocument<AccessRequest> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}