using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
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
    public class AgentsController : EntityController<AgentModel>
    {
        IAgentManager agentManager;
        IWebhookPublisher webhookPublisher;
        IAgentRepository agentRepo;
        IPersonRepository personRepo;
        IAspNetUsersRepository usersRepo;
        IAgentHeartbeatRepository agentHeartbeatRepo;
        private IHttpContextAccessor _accessor;

        /// <summary>
        /// AgentsController constructor
        /// </summary>
        /// <param name="agentRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="usersRepository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="agentManager"></param>
        /// <param name="accessor"></param>
        /// <param name="configuration"></param>
        public AgentsController(
            IAgentRepository agentRepository,
            IPersonRepository personRepository,
            IAspNetUsersRepository usersRepository,
            IAgentHeartbeatRepository agentHeartbeatRepository,
            IMembershipManager membershipManager,
            IWebhookPublisher webhookPublisher,
            ApplicationIdentityUserManager userManager,
            IAgentManager agentManager,
            IHttpContextAccessor accessor,
            IConfiguration configuration) : base(agentRepository, userManager, accessor, membershipManager, configuration)
        {
            this.agentRepo = agentRepository;
            this.personRepo = personRepository;
            this.usersRepo = usersRepository;
            this.agentHeartbeatRepo = agentHeartbeatRepository;
            this.agentManager = agentManager;
            this.agentManager.SetContext(SecurityContext);
            this.webhookPublisher = webhookPublisher;
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
        [ProducesResponseType(typeof(PaginatedList<AgentModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AgentModel> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
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
        public int? Count(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
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
        [HttpGet("{id}", Name = "GetAgentModel")]
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
                    view = agentManager.GetAgentDetails(view);
                }

                return actionResult;
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
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique agent id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AgentModel), StatusCodes.Status200OK)]
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
                //Name must be unique
                AgentModel namedAgent = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
                if (namedAgent != null)
                {
                    ModelState.AddModelError("Agent", "Agent Name Already Exists");
                    return BadRequest(ModelState);
                }

                Guid entityId = Guid.NewGuid();
                if (request.Id == null || !request.Id.HasValue || request.Id.Equals(Guid.Empty))
                    request.Id = entityId;

                //create Agent app user
                var user = new ApplicationUser()
                {
                    Name = request.Name,
                    UserName = request.UserName
                };

                var loginResult = await userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

                if (!loginResult.Succeeded)
                {
                    ModelState.AddModelError("Agent", "Failed to Create Agent User");
                    return BadRequest(ModelState);
                }
                else
                {
                    Person newPerson = new Person()
                    {
                        Name = request.Name,
                        IsAgent = true
                    };
                    var person = personRepo.Add(newPerson);

                   
                    if (person == null)
                    {
                        ModelState.AddModelError("Agent", "Failed to Create Agent User");
                        return BadRequest(ModelState);
                    }

                    //Update the user 
                    var registeredUser = userManager.FindByNameAsync(user.UserName).Result;
                    registeredUser.PersonId = (Guid)person.Id;
                    await userManager.UpdateAsync(registeredUser).ConfigureAwait(false);

                    //Post Agent entity
                    AgentModel newAgent = request.Map(request);
                    await webhookPublisher.PublishAsync("Agents.NewAgentCreated", newAgent.Id.ToString(), newAgent.Name).ConfigureAwait(false);
                    return await base.PostEntity(newAgent);
                }
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
        public async Task<IActionResult> Put(string id, [FromBody] AgentModel request)
        {
            try
            {
                Guid entityId = new Guid(id);
                
                var existingAgent = repository.GetOne(entityId);
                if (existingAgent == null) return NotFound();

                var namedAgent = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                if (namedAgent != null && namedAgent.Id != entityId)
                {
                    ModelState.AddModelError("Agent", "Agent Name Already Exists");
                    return BadRequest(ModelState);
                }

                if (existingAgent.Name != request.Name)
                {
                    Person person = personRepo.Find(0, 1).Items?.Where(p => p.Name == existingAgent.Name && p.IsAgent && (p.IsDeleted ?? false))?.FirstOrDefault();
                    if (person != null)
                    {
                        person.UpdatedBy = string.IsNullOrWhiteSpace(applicationUser?.Name) ? person.UpdatedBy : applicationUser?.Name;
                        person.Name = request.Name;
                        personRepo.Update(person);
                    }
                }

                existingAgent.Name = request.Name;
                existingAgent.MachineName = request.MachineName;
                existingAgent.MacAddresses = request.MacAddresses;
                existingAgent.IPAddresses = request.IPAddresses;
                existingAgent.IsEnabled = request.IsEnabled;
                existingAgent.CredentialId = request.CredentialId;

                await webhookPublisher.PublishAsync("Agents.AgentUpdated", existingAgent.Id.ToString(), existingAgent.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingAgent);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Agent", ex.Message);
                return BadRequest(ModelState);
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
            AgentModel agent = agentRepo.GetOne(new Guid(id));
            if (agent == null)
            {
                ModelState.AddModelError("Delete Agent", "No Agent was found with the specified Agent ID");
                return NotFound(ModelState);
            }

            bool childExists = agentManager.CheckReferentialIntegrity(id);
            if (childExists)
            {
                ModelState.AddModelError("Delete Agent", "Referential Integrity in Schedule or Job table, please remove those before deleting this agent.");
                return BadRequest(ModelState);
            }

            Person person = personRepo.Find(0, 1).Items?.Where(p => p.IsAgent && p.Name == agent.Name && !(p.IsDeleted ?? false))?.FirstOrDefault();
            var aspUser = usersRepo.Find(0, 1).Items?.Where(u => u.PersonId == person.Id)?.FirstOrDefault();

            if (aspUser == null)
            {
                ModelState.AddModelError("Delete Agent", "Something went wrong, could not find Agent User");
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByIdAsync(aspUser.Id);
            var deleteResult = await userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                ModelState.AddModelError("Delete Agent", "Something went wrong, unable to Delete Agent User");
                return BadRequest(ModelState);
            }

            agentManager.DeleteExistingHeartbeats(agent.Id ?? Guid.Empty);

            await webhookPublisher.PublishAsync("Agents.AgentDeleted", agent.Id.ToString(), agent.Name).ConfigureAwait(false);
            return await base.DeleteEntity(id);
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
            [FromBody] JsonPatchDocument<AgentModel> request)
        {
            Guid entityId = new Guid(id);

            var existingAgent = repository.GetOne(entityId);
            if (existingAgent == null) return NotFound();

            for (int i = 0; i < request.Operations.Count; i++)
            {
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var agent = repository.Find(null, d => d.Name.ToLower(null) == request.Operations[i].value.ToString().ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (agent != null)
                    {
                        ModelState.AddModelError("Agent", "Agent Name Already Exists");
                        return BadRequest(ModelState);
                    }

                    Person person = personRepo.Find(0, 1).Items?.Where(p => p.Name == existingAgent.Name && p.IsAgent && !(p.IsDeleted ?? false))?.FirstOrDefault();
                    person.UpdatedBy = string.IsNullOrWhiteSpace(applicationUser?.Name) ? person.UpdatedBy : applicationUser?.Name;
                    person.Name = request.Operations[i].value.ToString();
                    personRepo.Update(person);
                }
            }

            return await base.PatchEntity(id, request);
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

                ConnectedViewModel connectedViewModel = new ConnectedViewModel();
                var requestIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                var agent = agentRepo.FindAgent(request.MachineName, request.MacAddresses, requestIp, entityId);

                if (agent == null)
                {
                    ModelState.AddModelError("Connect", "Record does not exist or you do not have authorized access.");
                    return BadRequest(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    JsonPatchDocument<AgentModel> connectPatch = new JsonPatchDocument<AgentModel>();

                    connectPatch.Replace(e => e.IsConnected, true);
                    await base.PatchEntity(agent.Id.ToString(), connectPatch);
                }

                return new OkObjectResult(connectedViewModel.Map(agent));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Connect", ex.Message);
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
                var agent = this.agentRepo.FindAgent(request.MachineName, request.MacAddresses, requestIp, agentGuid);

                if (agent == null)
                {
                    return NotFound("Agent not found");
                }
                if (agent.Id != agentGuid)
                {
                    ModelState.AddModelError("Disconnect", "AgentId does not match existing agent");
                    return BadRequest(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("Disconnect", "Agent is already disconnected");
                    return BadRequest(ModelState);
                }

                JsonPatchDocument<AgentModel> disconnectPatch = new JsonPatchDocument<AgentModel>();

                disconnectPatch.Replace(e => e.IsConnected, false);
                return await base.PatchEntity(agentID, disconnectPatch);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Disconnect", ex.Message);
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
        [HttpPost("{AgentId}/AddHeartbeat")]
        [ProducesResponseType(typeof(AgentHeartbeat), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> AddHeartbeat([FromBody] AgentHeartbeat request, string agentId)
        {
            try
            {
                if (request == null)
                {
                    ModelState.AddModelError("Save", "No data passed");
                    return BadRequest(ModelState);
                }

                Guid entityId = Guid.NewGuid();
                if (request.Id == null || !request.Id.HasValue || request.Id.Equals(Guid.Empty))
                    request.Id = entityId;

                AgentModel agent = agentRepo.GetOne(new Guid(agentId));
                if (agent == null)
                {
                    return NotFound("The Agent ID provided does not match any existing Agents");
                }

                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("HeartBeat", "Agent is not connected");
                    return BadRequest(ModelState);
                }

                if (request.IsHealthy == false)
                {
                    await webhookPublisher.PublishAsync("Agents.UnhealthyReported", agent.Id.ToString(), agent.Name).ConfigureAwait(false);
                }

                //Add HeartBeat Values
                request.AgentId = new Guid(agentId);
                request.CreatedBy = applicationUser?.UserName;
                request.CreatedOn = DateTime.UtcNow;
                agentHeartbeatRepo.Add(request);
                var resultRoute = "GetAgentHeartbeat";

                return CreatedAtRoute(resultRoute, new { id = request.Id.Value.ToString("b") }, request);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Heartbeat", ex.Message);
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
        [HttpGet("{AgentId}/AgentHeartbeats", Name = "GetAgentHeartbeat")]
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
            AgentModel agent = agentRepo.GetOne(new Guid(agentId));
            if (agent == null)
            {
                return NotFound("The Agent ID provided does not match any existing Agents");
            }

            ODataHelper<AgentHeartbeat> oData = new ODataHelper<AgentHeartbeat>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;

            return Ok(agentHeartbeatRepo.Find(parentguid, oData.Filter, oData.Sort, oData.SortDirection, oData.Skip,
                oData.Top).Items.Where(a => a.AgentId == new Guid(agentId)));
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
        public List<JobAgentsLookup> GetLookup()
        {
            var agentList = repository.Find(null, x => x.IsDeleted == false);
            var agentLookup = from a in agentList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                              select new JobAgentsLookup
                                {
                                    AgentId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                    AgentName = a?.Name
                                };

            return agentLookup.ToList();
        }
    }
}
