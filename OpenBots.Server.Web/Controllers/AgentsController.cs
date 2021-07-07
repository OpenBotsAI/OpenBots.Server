using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for agents
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AgentsController : EntityController<Agent>
    {
        private readonly IAgentManager _agentManager;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IAgentRepository _agentRepo;
        private readonly IAgentHeartbeatRepository _agentHeartbeatRepo;
        private readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// AgentsController constructor
        /// </summary>
        /// <param name="agentRepository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="agentManager"></param>
        /// <param name="accessor"></param>
        /// <param name="configuration"></param>
        /// <param name="webhookPublisher"></param>
        public AgentsController(
            IAgentRepository agentRepository,
            IAgentHeartbeatRepository agentHeartbeatRepository,
            IMembershipManager membershipManager,
            IWebhookPublisher webhookPublisher,
            ApplicationIdentityUserManager userManager,
            IAgentManager agentManager,
            IHttpContextAccessor accessor,
            IConfiguration configuration) : base(agentRepository, userManager, accessor, membershipManager, configuration)
        {
            _agentRepo = agentRepository;
            _agentHeartbeatRepo = agentHeartbeatRepository;
            _agentManager = agentManager;
            _agentManager.SetContext(SecurityContext);
            _webhookPublisher = webhookPublisher;
            _accessor = accessor;
        }

        /// <summary>
        /// Provides a list of all Agents
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">OK,a Paginated list of all Agents</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all agents</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Agent>), StatusCodes.Status200OK)]
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
        /// Provides a view model list of all Agents and their most recent heartbeat information
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all Agents</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all Agents</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllAgentsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                ODataHelper<AllAgentsViewModel> oDataHelper = new ODataHelper<AllAgentsViewModel>();

                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_agentRepo.FindAllView(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a list of all AgentGroupMembers for the specified Agent 
        /// </summary>
        /// <param name="agentGroupId"> Specifies the id of the Agent</param>
        /// <response code="200">OK,a paginated list of GroupMembers</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all AgentGroupMembers with the specified Agent id</returns>
        [HttpGet("{agentGroupId}/GetAllGroupMembers")]
        [ProducesResponseType(typeof(PaginatedList<AgentGroupMember>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAllGroupMembers(string agentGroupId)
        {
            try
            {
                var result = _agentManager.GetAllMembersInGroup(agentGroupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a count of agents 
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, total count of agents</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all agents</returns>
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
        /// Provides agent details for a particular agent id
        /// </summary>
        /// <param name="id">Agent id</param>
        /// <response code="200">Ok, if an agent exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if agent id is not in proper format or proper Guid<response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no agent exists for the given agent id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Agent details for the given id</returns>
        [HttpGet("{id}", Name = "GetAgent")]
        [ProducesResponseType(typeof(AgentViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<AgentViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    AgentViewModel view = okResult.Value as AgentViewModel;
                    view = _agentManager.GetAgentDetails(view);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Fetches values for an Agent that matches the provided details
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, resolved Agent</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Agent details for the resolved Agent</returns>
        [HttpPost("Resolve")]
        [ProducesResponseType(typeof(ResolvedAgentResponseViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Resolve(ResolveAgentViewModel request)
        {
            try
            {
                ResolvedAgentResponseViewModel response = _agentManager.ResolveAgent(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new agent to the existing agents and create a new agent application user
        /// </summary>
        /// <remarks>
        /// Adds the agent with unique agent id to the existing agents
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new agent created and returned</response>
        /// <response code="400">Bad request, when the agent value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique agent id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Agent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] CreateAgentViewModel request)
        {
            try
            {
                _agentManager.CreateAgentUserAccount(request);

                //post agent entity
                Agent newAgent = request.Map(request);
                await _webhookPublisher.PublishAsync("Agents.NewAgentCreated", newAgent.Id.ToString(), newAgent.Name).ConfigureAwait(false);
                return await base.PostEntity(newAgent);
                
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an Agent 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an agent, when agent id and the new details of agent are given
        /// </remarks>
        /// <param name="id">Agent id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Agent details to be updated</param>
        /// <response code="200">Ok, if the agent details for the given agent id have been updated</response>
        /// <response code="400">Bad request, if the agent id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessabl entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateAgentViewModel request)
        {
            try
            {
                var existingAgent = _agentManager.UpdateAgent(id, request);
                var result = await base.PutEntity(id, existingAgent);
                await _webhookPublisher.PublishAsync("Agents.AgentUpdated", existingAgent.Id.ToString(), existingAgent.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes an agent with a specified id from the agents
        /// </summary>
        /// <param name="id">Agent id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when agent is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if agent id is null or empty Guid</response>
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
                Agent agent = _agentRepo.GetOne(new Guid(id));
                if (agent == null)
                {
                    throw new EntityDoesNotExistException("No agent was found with the specified agent id");
                }

                _agentManager.DeleteAgentDependencies(agent);

                var result = await base.DeleteEntity(id);
                await _webhookPublisher.PublishAsync("Agents.AgentDeleted", id, agent.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of agent
        /// </summary>
        /// <param name="id">Agent identifier</param>
        /// <param name="request">Value of the agent to be updated</param>
        /// <response code="200">Ok, if update of agent is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial agent values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Agent> request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingAgent = repository.GetOne(entityId);
                if (existingAgent == null) return NotFound();

                for (int i = 0; i < request.Operations.Count; i++)
                {
                    if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                    {
                        _agentManager.UpdateAgentName(existingAgent.Name, request.Operations[i].value.ToString().ToLower());
                    }
                }

                var result = await base.PatchEntity(id, request);
                await _webhookPublisher.PublishAsync("Agents.AgentUpdated", existingAgent.Id.ToString(), existingAgent.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides an agent id and name if the provided machine matches an agent and updates the isConnected field
        /// </summary>
        /// <response code="200">Ok, agent id</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">UnprocessableE entity</response>
        /// <returns>Connected view model that matches the provided machine details</returns>
        [HttpPatch("{agentID}/Connect")]
        [ProducesResponseType(typeof(ConnectedViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Connect(string agentID, [FromQuery] ConnectAgentViewModel request)
        {
            try
            {
                Guid entityId = new Guid(agentID);
                var requestIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                var connectedAgentDetails = _agentManager.ConnectAgent(agentID, requestIp, request);

                return Ok(connectedAgentDetails);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates the isConnected field if the disconnect details are correct
        /// </summary>
        /// <response code="200">Ok, if update of agent is successful</response>
        /// <response code="400">Badrequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response, if the isConnected field was updated</returns>
        [HttpPatch("{agentID}/Disconnect")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Disconnect(string agentID, [FromQuery] ConnectAgentViewModel request)
        {
            try
            {
                Guid? agentGuid = new Guid(agentID);
                var requestIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                _agentManager.DisconnectAgent(agentGuid, requestIp, request);

                return Ok();

            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Creates a new heartbeat for the specified AgentId
        /// </summary>
        /// <param name="agentId">Agent identifier</param>
        /// <param name="request">Heartbeat values to be updated</param>
        /// <response code="200">Ok, if update of agent is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Newly created Agent heartbeat</returns>
        [HttpPost("{agentId}/AddHeartbeat")]
        [ProducesResponseType(typeof(AgentHeartbeat), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> AddHeartbeat([FromBody] HeartbeatViewModel request, string agentId)
        {
            try
            {
                var newHeartBeat = _agentManager.PerformAgentHeartbeat(request, agentId);
                var resultRoute = "GetAgentHeartbeat";

                CreatedAtRoute(resultRoute, new { id = newHeartBeat.Id.Value.ToString("b") }, newHeartBeat);

                if (request.GetNextJob)
                {
                    var nextJob = _agentManager.GetNextJob(agentId);
                    return Ok(nextJob);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a list of heartbeat details for a particular agent id
        /// </summary>
        /// <param name="agentId">Agent id</param>
        /// <response code="200">Ok, if a heartbeat exists for the given agent id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if agent id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="422">Unprocessable entity</response>
        /// <response code="404">Not found, when no agent exists for the given agent id</response>
        /// <returns>Agent heaetbeat details for the given id</returns>
        [HttpGet("{agentId}/AgentHeartbeats", Name = "GetAgentHeartbeat")]
        [ProducesResponseType(typeof(PaginatedList<AgentHeartbeat>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AgentHeartbeats(
            string agentId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                Agent agent = _agentRepo.GetOne(new Guid(agentId));
                if (agent == null)
                {
                    return NotFound("The Agent ID provided does not match any existing Agents");
                }

                ODataHelper<AgentHeartbeat> oDataHelper = new ODataHelper<AgentHeartbeat>();

                Guid parentguid = Guid.Parse(agentId);

                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                var result = _agentHeartbeatRepo.FindAllHeartbeats(parentguid, oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all agents
        /// </summary>
        /// <response code="200">Ok, a lookup list of all agents</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Lookup list of all agents</returns>
        [HttpGet("GetLookup")]
        [ProducesResponseType(typeof(List<JobAgentsLookup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetLookup()
        {
            try
            {
                var agentList = repository.Find(null, x => x.IsDeleted == false);
                var agentLookup = from a in agentList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                  select new JobAgentsLookup
                                  {
                                      AgentId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                      AgentName = a?.Name
                                  };

                return Ok(agentLookup.ToList());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
