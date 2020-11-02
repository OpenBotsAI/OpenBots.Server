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
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AgentsController : EntityController<AgentModel>
    {
        IAgentManager agentManager;
        IAgentRepository agentRepo;
        IPersonRepository personRepo;
        IAspNetUsersRepository usersRepo;
        private IHttpContextAccessor _accessor;

        public AgentsController(
            IAgentRepository agentRepository,
            IPersonRepository personRepository,
            IAspNetUsersRepository usersRepository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IAgentManager agentManager,
            IHttpContextAccessor accessor,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(agentRepository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.agentRepo = agentRepository;
            this.personRepo = personRepository;
            this.usersRepo = usersRepository;
            this.agentManager = agentManager;
            this.agentManager.SetContext(base.SecurityContext);
            _accessor = accessor;
        }

        /// <summary>
        /// Provides a list of all Agents
        /// </summary>
        /// <response code="200">OK,a Paginated list of all Agents</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// /// <returns>Paginated list of all Agents </returns>
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
        /// Provides a Count of Agents 
        /// </summary>
        /// <response code="200">OK, total count of Agents</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Int contating the total number of Agents </returns>
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
        /// Provides an Agent details for a particular Agent Id.
        /// </summary>
        /// <param name="id">Agent id</param>
        /// <response code="200">OK, If an Agent exists with the given Id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">BadRequest,If Agent id is not in proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound, when no Agent exists for the given Agent id</response>
        /// <returns>Agent details for the given Id</returns>
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
        /// Adds a new Agent to the existing Agents and create a new Agent Application user
        /// </summary>
        /// <remarks>
        /// Adds the Agent with unique Agent Id to the existing Agents
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">OK,new Agent created and returned</response>
        /// <response code="400">BadRequest,When the Agent value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict,concurrency error</response> 
        /// <response code="422">UnprocessabileEntity,when a duplicate record is being entered.</response>
        /// <returns> newly created unique Agent Id with route name </returns>
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

                    //post Agent entity
                    AgentModel newAgent = request.Map(request);
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
        /// Provides an action to update an Agent, when Agent id and the new details of Agent are given
        /// </remarks>
        /// <param name="id">Agent Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="value">Agent details to be updated</param>
        /// <response code="200">OK, If the Agent details for the given Agent Id has been updated.</response>
        /// <response code="400">BadRequest,if the Agent Id is null or Id's don't match</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>OK response with the updated value</returns>
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
                    Person person = personRepo.Find(0, 1).Items?.Where(p => p.Name == existingAgent.Name && p.IsAgent && !(p.IsDeleted ?? false))?.FirstOrDefault();
                    person.UpdatedBy = string.IsNullOrWhiteSpace(applicationUser?.Name) ? person.UpdatedBy : applicationUser?.Name;
                    person.Name = request.Name;
                    personRepo.Update(person);
                }              

                existingAgent.Name = request.Name;
                existingAgent.MachineName = request.MachineName;
                existingAgent.MacAddresses = request.MacAddresses;
                existingAgent.IPAddresses = request.IPAddresses;
                existingAgent.IsEnabled = request.IsEnabled;
                existingAgent.LastReportedOn = request.LastReportedOn;
                existingAgent.LastReportedStatus = request.LastReportedStatus;
                existingAgent.LastReportedWork = request.LastReportedWork;
                existingAgent.LastReportedMessage = request.LastReportedMessage;
                existingAgent.IsHealthy = request.IsHealthy;
                existingAgent.CredentialId = request.CredentialId;

                return await base.PutEntity(id, existingAgent);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Agent", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes an Agent with a specified id from the Agents.
        /// </summary>
        /// <param name="id">Agent ID to be deleted- throws BadRequest if null or empty Guid/</param>
        /// <response code="200">OK,when Agent is softdeleted,( isDeleted flag is set to true in DB) </response>
        /// <response code="400">BadRequest,If Agent Id is null or empty Guid</response>
        /// <response code="403">Forbidden </response>
        /// <returns>OK response with deleted value </returns>
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

            bool isChildExists = agentManager.CheckReferentialIntegrity(id);
            if (isChildExists)
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

            personRepo.SoftDelete(person.Id ?? Guid.Empty);

            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of Agent.
        /// </summary>
        /// <param name="id">Agent identifier</param>
        /// <param name="value">Value of the Agent to be updated.</param>
        /// <response code="200">OK,If update of Agent is successful. </response>
        /// <response code="400">BadRequest,if the Id is null or Id's dont match.</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">Unprocessable entity,validation error</response>
        /// <returns>Ok response, if the partial Agent values has been updated</returns>

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
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
        /// Provides an Agent id and Name if the provided machine matches an Agent and updates the isConnected field
        /// </summary>
        /// <response code="200">OK,AgentId</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>connectedViewModel that matches the provided machine details </returns>
        [HttpPatch("Connect")]
        [ProducesResponseType(typeof(ConnectedViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Connect([FromQuery] ConnectAgentViewModel request = null)
        {
            try
            {
                ConnectedViewModel connectedViewModel = new ConnectedViewModel();
                var requestIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                var agent = agentRepo.FindAgent(request.MachineName, request.MacAddresses, requestIp);

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
        /// <response code="200">OK,If update of Agent is successful</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Ok response, if the isConnected field was updated </returns>
        [HttpPatch("Disconnect")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Disconnect([FromQuery] DisConnectAgentViewModel request = null)
        {
            try
            {
                var requestIp = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
                var agent = this.agentRepo.FindAgent(request.MachineName, request.MacAddresses, requestIp);

                if (agent == null)
                {
                    return NotFound("Agent not found");
                }
                if (agent.Id != request.AgentId)
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
                return await base.PatchEntity(request.AgentId.ToString(), disconnectPatch);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Disconnect", ex.Message);
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Performs a Heatbeat on Agent id.
        /// </summary>
        /// <param name="id">Agent identifier</param>
        /// <param name="request">Heartbeat values to be updated.</param>
        /// <response code="200">OK,If update of Agent is successful. </response>
        /// <response code="400">BadRequest,if the Id is null or Id's dont match.</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">Unprocessable entity,validation error</response>
        /// <returns>Ok response, if the heartbeat Agent values have been updated</returns>
        [HttpPatch("{id}/Heartbeat")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Heartbeat(string id,
            [FromBody] HeartbeatViewModel request)
        {
            try
            {
                if (request == null)
                {
                    ModelState.AddModelError("HeartBeat", "No data passed");
                    return BadRequest(ModelState);
                }

                Guid entityId = new Guid(id);
                var agent = agentRepo.GetOne(entityId);

                if (agent == null)
                {
                    return NotFound("The Agent ID provided does not match any existing Agents");
                }

                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("HeartBeat", "Agent is not connected");
                    return BadRequest(ModelState);
                }

                JsonPatchDocument<AgentModel> heartbeatPatch = new JsonPatchDocument<AgentModel>();

                heartbeatPatch.Replace(e => e.LastReportedOn, request.LastReportedOn);
                heartbeatPatch.Replace(e => e.LastReportedStatus, request.LastReportedStatus);
                heartbeatPatch.Replace(e => e.LastReportedWork, request.LastReportedWork);
                heartbeatPatch.Replace(e => e.LastReportedMessage, request.LastReportedMessage);
                heartbeatPatch.Replace(e => e.IsHealthy, request.IsHealthy);

                return await base.PatchEntity(id, heartbeatPatch);
            }

            catch (Exception ex)
            {
                ModelState.AddModelError("Heartbeat", ex.Message);
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all agents
        /// </summary>
        /// <response code="200">Ok, a lookup list of all agents</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <returns>Lookup list of all agents</returns>
        // GET: api/v1/agents/getlookup
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
