using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Web;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers.PersonApi
{
    /// <summary>
    /// Controller for organization membership
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/People/{personId}/Organizations")]
    [ApiController]
    [Authorize]
    public class MembershipController : EntityController<Organization>
    {
        IOrganizationMemberRepository orgMemberRepository;
        IMembershipManager membershipManager;

        /// <summary>
        /// MembershipController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="orgMemberRepository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public MembershipController(
            IOrganizationRepository repository, 
            IOrganizationMemberRepository orgMemberRepository,
            IMembershipManager membershipManager, 
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.membershipManager = membershipManager;
            this.orgMemberRepository = orgMemberRepository;
            this.membershipManager.SetContext(base.SecurityContext);
            this.orgMemberRepository.SetContext(base.SecurityContext);
        }

        /// <summary>
        /// Provides a list of organizations that can be presented to the user as suggestions to apply for access request
        /// </summary>
        /// <remarks>
        /// This method will return all organizations that are visible to users with a certain email domain
        /// All the emails of the user are matched to the allowed domains of organization
        /// </remarks>
        /// <param name="personId" >Id of the currently logged in user. If the user id is not the same, then the request will be rejected</param>
        /// <returns>Paginated list of organizations that are being suggested. The object organization will not have any child objects</returns>
        /// <response code="400">Bad request, if no person id is provided or it is not a proper Guid</response>
        /// <response code="200">Ok, paginated list of organizations that are being suggested. The object organization will not have any child objects</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        [HttpGet("Suggestions")]
        [ProducesResponseType(typeof(PaginatedList<OrganizationListing>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceBadRequest), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetSuggestions(
            string personId)
        {
            Guid personGuid = Guid.Empty;
            if (!Guid.TryParse(personId, out personGuid))
            {
                this.ModelState.AddModelError(nameof(personId), "");
                return BadRequest(ModelState);
            }

            try
            {
                var listOfOrg = membershipManager.Suggestions(personGuid);
                return Ok(listOfOrg);
            }
            catch(Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Get cards for organizations a person has access to
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <response code="200">Ok, paginated list of organizations that user has access to</response>
        /// <response code="400">Bad request, if the person id is not entered or an improper Guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Organization cards for all organizations user has access to</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<OrganizationCard>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string personId)
        {
            try
            {
                Guid personGuid = SecurityContext.PersonId;
                return Ok(membershipManager.MyOrganizations(personGuid, true));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Get organizations user has access to
        /// </summary>
        /// <param name="startsWith">Search criteria</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <response code="200">Ok, paginated listing of all organizations the user has access to</response>
        /// <response code="400">Bad request, if the person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>List of all organizations user has access to</returns>
        [HttpGet("Search")]
        [ProducesResponseType(typeof(PaginatedList<OrganizationListing>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Get(
            [FromQuery] string startsWith = "",
            [FromQuery(Name ="$skip")] int skip = 0,
            [FromQuery(Name = "$take")] int take = 10)
        {
            try
            {
                Guid personGuid = SecurityContext.PersonId;
                return Ok(membershipManager.Search(personGuid, startsWith, skip, take, false));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Request for joining an organization
        /// </summary>
        /// <param name="personId">User id of the person logged in</param>
        /// <param name="id">Organization id that the user wants to be a member of</param>
        /// <response code="200">Ok, if the user has been successfully added to the organization</response>
        /// <response code="400">Bad request, if no person id or organization id is provided or it is not a proper Guid</response>
        /// <returns>Newly created access request</returns>
        [HttpPost("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(string personId, string id)
        {
            Guid personGuid = Guid.Empty;
            if (!Guid.TryParse(personId, out personGuid))
            {
                ModelState.AddModelError("JoinOrganization", "Person Id has errors");
                return BadRequest(ModelState);
            }
            Guid orgGuid = Guid.Empty;
            if (!Guid.TryParse(id, out orgGuid))
            {
                ModelState.AddModelError("JoinOrganization", "Organization Id has errors");
                return BadRequest(ModelState);
            }

            try
            {
                if (SecurityContext == null)
                {
                    ModelState.AddModelError("JoinOrganization", "Join organization failed. Logged in user no longer exists in system.");
                    return BadRequest(ModelState);
                }

                var accessRequest =  membershipManager.JoinOrganization(personGuid, orgGuid);
                if (accessRequest == null)
                {
                    ModelState.AddModelError("JoinOrganization", "Organization request pending");
                    return BadRequest(ModelState);
                }

                return Ok(accessRequest);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Revoke admin permission
        /// </summary>
        /// <param name="personId">User who's admin permission needs to be revoked</param>
        /// <param name="id">Organization id</param>
        /// <returns>Organization member details</returns>
        [HttpPut("{id}/RevokeAdmin")]
        [Produces("application/json")]
        public async Task<IActionResult> RevokeAdmin(string personId, string id)
        {
            Guid personGuid = Guid.Empty;
            if (!Guid.TryParse(personId, out personGuid))
            {
                ModelState.AddModelError("RevokeAdminPermission", "Person Id has errors");
                return BadRequest(ModelState);
            }
            Guid orgGuid = Guid.Empty;
            if (!Guid.TryParse(id, out orgGuid))
            {
                ModelState.AddModelError("RevokeAdminPermission", "Organization Id has errors");
                return BadRequest(ModelState);
            }

            //Cannot revoke for self i.e. logged in user
            if (SecurityContext.PersonId == personGuid) {
                ModelState.AddModelError("RevokeAdminPermission", "Cannot revoke admin permission for self");
                return BadRequest(ModelState);
            }

            try
            {
                var orgMember = membershipManager.RevokeAdminPermisson(orgGuid, personGuid, SecurityContext);
                return Ok(orgMember);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Grant admin permission to non-admin users of the organization
        /// </summary>
        /// <param name="personId">User who's admin permission needs to be grant</param>
        /// <param name="id">Organization id</param>
        /// <returns>Organization member details</returns>
        [HttpPut("{id}/GrantAdmin")]
        [Produces("application/json")]
        public async Task<IActionResult> GrantAdmin(string personId, string id)
        {
            Guid personGuid = Guid.Empty;
            if (!Guid.TryParse(personId, out personGuid))
            {
                ModelState.AddModelError("GrantAdmin", "Person Id has errors");
                return BadRequest(ModelState);
            }
            Guid orgGuid = Guid.Empty;
            if (!Guid.TryParse(id, out orgGuid))
            {
                ModelState.AddModelError("GrantAdmin", "Organization Id has errors");
                return BadRequest(ModelState);
            }

            //Cannot revoke for self i.e. logged in user
            if (SecurityContext.PersonId == personGuid)
            {
                ModelState.AddModelError("GrantAdmin", "Cannot grant admin permission for self");
                return BadRequest(ModelState);
            }

            try
            {
                bool IsOrgAdmin = Array.Exists(SecurityContext.OrganizationId, o => o.CompareTo(orgGuid) == 0);

                if (IsOrgAdmin)
                {
                    var orgMember = membershipManager.GrantAdminPermission(orgGuid, personGuid);
                    return Ok(orgMember);
                }
                else {
                    ModelState.AddModelError("GrantAdmin", "Only organization admin can grant admin rights");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Leave a particular organization
        /// </summary>
        /// <param name="personId">Id of the currently logged in user. If the user id is not the same, then the request will be rejected</param>
        /// <param name="id">Id of the organization that the user wants to delete the membership from</param>
        /// <returns>Ok response if the soft delete is successful</returns>
        /// <response code="400">Bad request, if no organization id or person id is provided or it is not a proper Guid</response>
        /// <responce code="200">Ok, when membership to organization has been soft deleted for the logged in user</responce>
        /// <returns>Ok, if the membership to the organization is deleted for the user</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string personId, string id)
        {
            Guid personGuid = Guid.Empty;
            if (!Guid.TryParse(personId, out personGuid))
            {
                ModelState.AddModelError("Delete", "Person Id has errors");
                return BadRequest(ModelState);
            }

            Guid orgGuid = Guid.Empty;
            if (!Guid.TryParse(id, out orgGuid))
            {
                ModelState.AddModelError("Delete", "Organization Id has errors");
                return BadRequest(ModelState);
            }

            var OrgMembers = orgMemberRepository.Find(null, a => a.OrganizationId == orgGuid)?.Items;
            var OrgMemberCount = OrgMembers?.Count;
            var OrgAdminCount = OrgMembers?.Where(a => a.IsAdministrator == true)?.Count();

            var member = OrgMembers?.Where(p => p.PersonId == personGuid).FirstOrDefault();

            if (member?.IsAdministrator == true)
            {
                if (OrgAdminCount < 2)
                {
                    ModelState.AddModelError("ExitOrg", "Sorry - Unable to exit from the organization since you are the only Admin of this organization, please designate another Admin before exiting this organization.");
                    return BadRequest(ModelState);
                }
                else if (OrgMemberCount < 2)
                {
                    ModelState.AddModelError("ExitOrg", "Admin cannot exit organization if no other members exist.");
                    return BadRequest(ModelState);
                }
            }

            try
            {
                if (member != null && member.Id.HasValue)
                    orgMemberRepository.Delete(member.Id.Value);
                else
                    throw new UnauthorizedAccessException();

                return Ok();
            }
            catch(Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}