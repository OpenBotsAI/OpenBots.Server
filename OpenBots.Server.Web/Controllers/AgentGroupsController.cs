using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for AgentGroups
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AgentGroupsController : EntityController<AgentGroup>
    {
        private readonly IAgentGroupManager _agentGroupsManager;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IAgentGroupRepository _agentGroupRepository;

        /// <summary>
        /// AgentGroupsController constructor
        /// </summary>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="accessor"></param>
        /// <param name="configuration"></param>
        public AgentGroupsController(
            IAgentGroupRepository agentGroupRepository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor accessor,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IAgentGroupManager agentGroupsManager,
            IWebhookPublisher webhookPublisher) : base(agentGroupRepository, userManager, accessor, membershipManager, configuration)
        {
            _agentGroupsManager = agentGroupsManager;
            _webhookPublisher = webhookPublisher;
            _agentGroupRepository = agentGroupRepository;
        }

        /// <summary>
        /// Provides a list of all AgentGroups
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">OK,a paginated list of all AgentGroups</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all AgentGroups</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<AgentGroup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                return Ok(base.GetMany());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
   
        /// <summary>
        /// Provides a count of AgentGroups 
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, total count of AgentGroups</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all AgentGroups</returns>
        [HttpGet("Count")]
        [ProducesResponseType(typeof(int?), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Count(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                return Ok(base.Count());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides agent details for a particular AgentGroup id
        /// </summary>
        /// <param name="id">AgentGroup id</param>
        /// <response code="200">Ok, if an AgentGroup exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if AgentGroup id is not in proper format or proper Guid<response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no agent exists for the given AgentGroup id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>AgentGroup details for the given id</returns>
        [HttpGet("{id}", Name = "GetAgentGroup")]
        [ProducesResponseType(typeof(AgentGroup), StatusCodes.Status200OK)]
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
                return await base.GetEntity(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides an AgentGroup's view model details for a particular AgentGroup id
        /// </summary>
        /// <param name="id"> Specifies the id of the AgentGroup</param>
        /// <response code="200">OK,a paginated list of all AgentGroups view model</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all AgentGroup views for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(PaginatedList<AgentGroupViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> View(string id)
        {
            try
            {
                IActionResult actionResult = await base.GetEntity<AgentGroupViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    AgentGroupViewModel view = okResult.Value as AgentGroupViewModel;
                    view.AgentGroupMembers = _agentGroupsManager.GetMembersView(id);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new AgentGroup to the existing AgentGroups
        /// </summary>
        /// <remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new AgentGroup created and returned</response>
        /// <response code="400">Bad request, when the AgentGroup value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique AgentGroup id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Agent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] AgentGroup request)
        {
            try
            {
                var result = await base.PostEntity(request);           
                await _webhookPublisher.PublishAsync("AgentGroups.NewAgentGroupCreated", request.Id.ToString(), request.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates the AgentGroupMembers of the specified AgentGroup id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, new AgentGroupMembers created and returned</response>
        /// <response code="400">Bad request, when the request value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created AgentGroupMembers</returns>
        [HttpPut("{id}/UpdateGroupMembers")]
        [ProducesResponseType(typeof(Agent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> UpdateGroupMembers(string id, [FromBody] IEnumerable<AgentGroupMember> request)
        {
            try
            {
                var newGroupMembers = _agentGroupsManager.UpdateGroupMembers(id, request);
                return Ok(newGroupMembers);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an AgentGroup 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an AgentGroup, when AgentGroup id and the new details of AgentGroup are given
        /// </remarks>
        /// <param name="id">AgentGroup id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">AgentGroup details to be updated</param>
        /// <response code="200">Ok, if the AgentGroup details for the given AgentGroup id have been updated</response>
        /// <response code="400">Bad request, if the AgentGroup id is null or ids don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] AgentGroup request)
        {
            try
            {
                var existingAgentGroup = _agentGroupsManager.UpdateAgentGroup(id, request);
                var result = await base.PutEntity(id, existingAgentGroup);

                await _webhookPublisher.PublishAsync("AgentGroups.AgentGroupUpdated", existingAgentGroup.Id.ToString(), existingAgentGroup.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes an AgentGroup with a specified id
        /// </summary>
        /// <param name="id">AgentGroup id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when AgentGroup is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if AgentGroup id is null or empty Guid</response>
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
            try
            {
                var agentGroup = repository.GetOne(Guid.Parse(id));
                if (agentGroup == null)
                {
                    ModelState.AddModelError("AgentGroup", "AgentGroup cannot be found or does not exist.");
                    return NotFound(ModelState);
                }
                var result = await base.DeleteEntity(id);
                _agentGroupsManager.DeleteGroupMembers(id);
                await _webhookPublisher.PublishAsync("AgentGroups.AgentGroupDeleted", id, agentGroup.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of an AgentGroup
        /// </summary>
        /// <param name="id">AgentGroup identifier</param>
        /// <param name="request">Value of the AgentGroup to be updated</param>
        /// <response code="200">Ok, if update of AgentGroup is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial AgentGroup values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<AgentGroup> request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingAgentGroup = _agentGroupRepository.GetOne(entityId);
                if (existingAgentGroup == null)
                {
                    throw new EntityDoesNotExistException("No agent group exists with the specified id");
                }

                _agentGroupsManager.AttemptPatchUpdate(request, entityId);
                var result = await base.PatchEntity(id, request);
                await _webhookPublisher.PublishAsync("AgentGroups.AgentGroupUpdated", existingAgentGroup.Id.ToString(), existingAgentGroup.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}