using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for IPFencing
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class IPFencingController : EntityController<IPFencing>
    {
        private readonly IOrganizationRepository organizationRepository;
        private readonly IIPFencingRepository iPFencingRepository;
        private readonly IIPFencingManager iPFencingManager;
        private readonly IPFencingOptions iPFencingOptions;


        /// <summary>
        /// IPFencing controller's constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public IPFencingController(
            IIPFencingRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IOrganizationRepository organizationRepository,
            IIPFencingManager iPFencingManager) : base(repository, userManager, httpContextAccessor,
                membershipManager, configuration)
        {
            this.organizationRepository = organizationRepository;
            this.iPFencingRepository = repository;
            this.iPFencingManager = iPFencingManager;
            iPFencingOptions = configuration.GetSection(IPFencingOptions.IPFencing).Get<IPFencingOptions>();
        }

        /// <summary>
        /// Provides a list of all IPFencings
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <response code="200">Ok, a paginated list of all IPFencings</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all IPFencings</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<IPFencing>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<IPFencing> Get(
            string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany(organizationId);
        }

        /// <summary>
        /// Provides an IPFencing's details for a particular IPFencing ID
        /// </summary>
        /// <param name="id">IPFencing id</param>
        /// <param name="organizationId"></param>
        /// <response code="200">Ok, if an IPFencing exists with the given id and organization id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if IPFencing request was unable to be processed </response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no IPFencing exists for the given IPFencing id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>IPFencing details for the given id</returns>
        [HttpGet("{id}", Name = "GetIPFencing")]
        [ProducesResponseType(typeof(IPFencing), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string organizationId, string id)
        {
            try
            {
                return await base.GetEntity(id,organizationId);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new IPFencing rule with the specified organization ID
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="request">Json containing IPFencing model </param>
        /// <response code="200">Ok, if IPFencing rule has been created</response>
        /// <response code="400">Bad request, if IPFencing request was unable to be processed</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, when IPFencing with the particular id already exists</response>
        /// <response code="422">Unprocessable entity, validation error or cannot insert duplicate constraint</response>
        /// <returns>Ok response with newly created IPFencing record</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateIPFencingViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Post(string organizationId, [FromBody] CreateIPFencingViewModel request)
        {
            try
            {
                var ipCheck = iPFencingOptions.IPFencingCheck;
                if (ipCheck.Equals("Disabled"))
                {
                    ModelState.AddModelError("Post", "IPFencing rule could not be added because IPFencingCheck is disabled");
                    return BadRequest(ModelState);
                }

                IPFencing iPFencing = request.Map(request);
                iPFencing.OrganizationId = Guid.Parse(organizationId);

                if (iPFencingManager.CanBeAdded(iPFencing))
                {
                    return await base.PostEntity(iPFencing);
                }
                else
                {
                    ModelState.AddModelError("Post", "IPFencing rule could not be added. The Organization's" +
                        "settings may not support this rule");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }          
        }

        /// <summary>
        /// Update the IPFencing rule
        /// </summary>
        /// <remarks>Updates the IPFencing with the particular id for the given organization</remarks>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">IPFencing id</param>
        /// <param name="request">New value of the IPFencing to be updated</param>
        /// <response code="200">Ok, if the update of the IPFencing for the particular id has been successful</response>
        /// <response code="400">Bad request, if IPFencing request was unable to be processed</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="404">Not found, if no IPFencing exists for the given id</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable Entity, validation error</response>
        /// <returns>Ok response with the updated IPFencing details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] IPFencing request)
        {
            var ipCheck = iPFencingOptions.IPFencingCheck;
            if (ipCheck.Equals("Disabled"))
            {
                ModelState.AddModelError("Post", "IPFencing rule could not be updated because IPFencingCheck is disabled");
                return BadRequest(ModelState);
            }

            Guid entityId = new Guid(id);
            var iPFencing = repository.GetOne(entityId);
            if (iPFencing == null)
            {
                ModelState.AddModelError("Update", "No IPFencing was found for the specified ID");
                return NotFound(ModelState);
            }

            if (iPFencing.OrganizationId != Guid.Parse(organizationId))
            {
                ModelState.AddModelError("Update", "The provided organization id does not match this IPFencing's current " +
                    "organization");
                return BadRequest(ModelState);
            }

            iPFencing.Usage = request.Usage;
            iPFencing.Rule = request.Rule;
            iPFencing.IPAddress = request.IPAddress;
            iPFencing.IPRange = request.IPRange;
            iPFencing.HeaderName = request.HeaderName;
            iPFencing.HeaderValue = request.HeaderValue;
            iPFencing.OrganizationId = request.OrganizationId;

            return await base.PutEntity(id, iPFencing);
        }

        /// <summary>
        /// Deletes an IPFencing rule with a specified id from the organization.
        /// </summary>
        /// <param name="id">IPFencing id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when IPFencing is soft deleted </response>
        /// <response code="400">Bad request, if IPFencing id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string organizationId, string id)
        {
            var ipCheck = iPFencingOptions.IPFencingCheck;
            if (ipCheck.Equals("Disabled"))
            {
                ModelState.AddModelError("Post", "IPFencing rule could not be deleted because IPFencingCheck is disabled");
                return BadRequest(ModelState);
            }

            Guid entityId = new Guid(id);
            var iPFencing = repository.GetOne(entityId, Guid.Parse(organizationId));

            if (iPFencing == null)
            {
                ModelState.AddModelError("Delete", "No IPFencing was found for the specified ID");
                return NotFound(ModelState);
            }

            if (iPFencing.OrganizationId != Guid.Parse(organizationId))
            {
                ModelState.AddModelError("Delete", "The provided organization id does not match this IPFencing's current " +
                    "organization");
                return BadRequest(ModelState);
            }

            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of an IPFencing entity.
        /// </summary>
        /// <param name="id">IPFencing identifier</param>
        /// <param name="value">Details of IPFencing patch</param>
        /// <response code="200">Ok, if update of IPFencing is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial IPFencing details have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string organizationId, string id, [FromBody] JsonPatchDocument<IPFencing> value)
        {
            var ipCheck = iPFencingOptions.IPFencingCheck;
            if (ipCheck.Equals("Disabled"))
            {
                ModelState.AddModelError("Post", "IPFencing rule could not be updated because IPFencingCheck is disabled");
                return BadRequest(ModelState);
            }

            Guid entityId = new Guid(id);
            var iPFencing = repository.GetOne(entityId);

            if (iPFencing == null)
            {
                ModelState.AddModelError("Patch", "No IPFencing was found for the specified ID");
                return NotFound(ModelState);
            }

            if (iPFencing.OrganizationId != Guid.Parse(organizationId))
            {
                ModelState.AddModelError("Patch", "The provided organization id does not match this IPFencing's current " +
                    "organization");
                return BadRequest(ModelState);
            }

            return await base.PatchEntity(id, value);
        }
    }
}