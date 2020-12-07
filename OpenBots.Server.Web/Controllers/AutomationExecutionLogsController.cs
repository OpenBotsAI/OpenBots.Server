using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AutomationExecutionLogsController : EntityController<AutomationExecutionLog>
    {
        readonly IAgentRepository agentRepository;
        IAutomationExecutionLogManager automationExecutionLogManager;
        public AutomationExecutionLogsController(
            IAutomationExecutionLogRepository repository,
            IAgentRepository agentRepository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IAutomationExecutionLogManager automationExecutionLogManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.automationExecutionLogManager = automationExecutionLogManager;
            this.automationExecutionLogManager.SetContext(base.SecurityContext);
            this.agentRepository = agentRepository;
        }

        /// <summary>
        /// Provides a list of all AutomationExecutionLogs
        /// </summary>
        /// <response code="200">OK,a Paginated list of all AutomationExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Paginated list of all AutomationExecutionLogs </returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<AutomationExecutionLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AutomationExecutionLog> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a viewmodel list of all AutomationExecutionLogs
        /// </summary>
        /// <response code="200">OK,a Paginated list of all AutomationExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Paginated list of all AutomationExecutionLogs </returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AutomationExecutionViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AutomationExecutionViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {

            ODataHelper<AutomationExecutionViewModel> oData = new ODataHelper<AutomationExecutionViewModel>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;
            var newNode = oData.ParseOrderByQuery(queryString);
            if (newNode == null)
                newNode = new OrderByNode<AutomationExecutionViewModel>();

            Predicate<AutomationExecutionViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<AutomationExecutionViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return automationExecutionLogManager.GetAutomationAndAgentNames(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
        }

        /// <summary>
        /// Provides a Count of AutomationExecutionLogs 
        /// </summary>
        /// <response code="200">OK, total count of AutomationExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Int contating the total number of AutomationExecutionLogs </returns>
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
        /// Provides a AutomationExecutionLog's details for a particular AutomationExecutionLog Id.
        /// </summary>
        /// <param name="id">AutomationExecutionLog id</param>
        /// <response code="200">OK, If a AutomationExecutionLog exists with the given Id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">BadRequest,If AutomationExecutionLog ID is not in the proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound, when no AutomationExecutionLog exists for the given AutomationExecutionLog ID</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>AutomationExecutionLog details for the given ID</returns>
        [HttpGet("{id}", Name = "GetAutomationExecutionLog")]
        [ProducesResponseType(typeof(AutomationExecutionLog), StatusCodes.Status200OK)]
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
                return await base.GetEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a AutomationExecution's details for a particular AutomationExecution id
        /// </summary>
        /// <param name="id">AutomationExecution id</param>
        /// <response code="200">OK, if a AutomationExecution exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">BadRequest, if AutomationExecution ID is not in the proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound, when no AutomationExecution exists for the given AutomationExecution ID</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>AutomationExecution details for the given ID</returns>
        [HttpGet("View/{id}")]
        [ProducesResponseType(typeof(AutomationExecutionViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> View(string id)
        {
            try
            {
                IActionResult actionResult = await base.GetEntity<AutomationExecutionViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    AutomationExecutionViewModel view = okResult.Value as AutomationExecutionViewModel;
                    view = automationExecutionLogManager.GetExecutionView(view);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new AutomationExecutionLog to the existing AutomationExecutionLogs
        /// </summary>
        /// <remarks>
        /// Adds the AutomationExecutionLog with unique AutomationExecutionLog Id to the existing AutomationExecutionLogs
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">OK,new AutomationExecutionLog created and returned</response>
        /// <response code="400">BadRequest,When the AutomationExecutionLog value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict,concurrency error</response> 
        /// <response code="422">UnprocessabileEntity,when a duplicate record is being entered.</response>
        /// <returns> newly created unique AutomationExecutionLog Id with route name </returns>
        [HttpPost]
        [ProducesResponseType(typeof(AutomationExecutionLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] AutomationExecutionLog request)
        {
            try
            {
                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Allows Agent to add a new AutomationExecutionLog to the existing AutomationExecutionLogs
        /// </summary>
        /// <remarks>
        /// Agent is able to Add the AutomationExecutionLog if the Agent is Connected
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">OK,new AutomationExecutionLog created and returned</response>
        /// <response code="400">BadRequest,When the AutomationExecutionLog value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict,concurrency error</response> 
        /// <response code="422">UnprocessabileEntity,when a duplicate record is being entered.</response>
        /// <returns> newly created unique AutomationExecutionLog Id with route name </returns>
        [AllowAnonymous]
        [HttpPost("StartAutomation")]
        [ProducesResponseType(typeof(AutomationExecutionLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> StartAutomation([FromBody] AutomationExecutionLog request)
        {
            try
            {
                var agent = agentRepository.Find(null, a=> a.Id == request.AgentID)?.Items?.FirstOrDefault();

                if (agent == null)
                {
                    ModelState.AddModelError("StartAutomation", "Agent not found");
                    return NotFound(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("StartAutomation", "Agent is not connected");
                    return BadRequest(ModelState);
                }
                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a AutomationExecutionLog 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a AutomationExecutionLog, when AutomationExecutionLog id and the new details of AutomationExecutionLog are given
        /// </remarks>
        /// <param name="id">AutomationExecutionLog Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">AutomationExecutionLog details to be updated</param>
        /// <response code="200">OK, If the AutomationExecutionLog details for the given AutomationExecutionLog Id has been updated.</response>
        /// <response code="400">BadRequest,if the AutomationExecutionLog Id is null or Id's don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] AutomationExecutionLog request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingAutomationExecutionLog = repository.GetOne(entityId);
                if (existingAutomationExecutionLog == null) return NotFound();

                existingAutomationExecutionLog.JobID = request.JobID;
                existingAutomationExecutionLog.AutomationID = request.AutomationID;
                existingAutomationExecutionLog.AgentID = request.AgentID;
                existingAutomationExecutionLog.StartedOn = request.StartedOn;
                existingAutomationExecutionLog.CompletedOn = request.CompletedOn;
                existingAutomationExecutionLog.Trigger = request.Trigger;
                existingAutomationExecutionLog.TriggerDetails = request.TriggerDetails;
                existingAutomationExecutionLog.Status = request.Status;
                existingAutomationExecutionLog.HasErrors = request.HasErrors;
                existingAutomationExecutionLog.ErrorMessage = request.ErrorMessage;
                existingAutomationExecutionLog.ErrorDetails = request.ErrorDetails;

                return await base.PutEntity(id, existingAutomationExecutionLog);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("AutomationExecutionLog", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Agent is able to update a AutomationExecutionLog End status
        /// </summary>
        /// <remarks>
        /// Provides an action to update a AutomationExecutionLog, when AutomationExecutionLog id and the new details of AutomationExecutionLog are given
        /// </remarks>
        /// <param name="id">AutomationExecutionLog Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">AutomationExecutionLog details to be updated</param>
        /// <response code="200">OK, If the AutomationExecutionLog details for the given AutomationExecutionLog Id has been updated.</response>
        /// <response code="400">BadRequest,if the AutomationExecutionLog Id is null or Id's don't match</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>OK response with the updated value</returns>
        [AllowAnonymous]
        [HttpPut("{id}/EndAutomation")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> EndAutomation(string id, [FromBody] AutomationExecutionLog request)
        {
            try
            {
                var agent = agentRepository.Find(null, a => a.Id == request.AgentID)?.Items?.FirstOrDefault();

                if (agent == null)
                {
                    ModelState.AddModelError("StartAutomation", "Agent not found");
                    return NotFound(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("StartAutomation", "Agent is not connected");
                    return BadRequest(ModelState);
                }
                Guid entityId = new Guid(id);

                var existingAutomationExecutionLog = repository.GetOne(entityId);
                if (existingAutomationExecutionLog == null) return NotFound();

                existingAutomationExecutionLog.JobID = request.JobID;
                existingAutomationExecutionLog.AutomationID = request.AutomationID;
                existingAutomationExecutionLog.AgentID = request.AgentID;
                existingAutomationExecutionLog.StartedOn = request.StartedOn;
                existingAutomationExecutionLog.CompletedOn = request.CompletedOn;
                existingAutomationExecutionLog.Trigger = request.Trigger;
                existingAutomationExecutionLog.TriggerDetails = request.TriggerDetails;
                existingAutomationExecutionLog.Status = request.Status;
                existingAutomationExecutionLog.HasErrors = request.HasErrors;
                existingAutomationExecutionLog.ErrorMessage = request.ErrorMessage;
                existingAutomationExecutionLog.ErrorDetails = request.ErrorDetails;

                return await base.PutEntity(id, existingAutomationExecutionLog);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("AutomationExecutionLog", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes a AutomationExecutionLog with a specified id from the AutomationExecutionLog.
        /// </summary>
        /// <param name="id">AutomationExecutionLog ID to be deleted- throws BadRequest if null or empty Guid/</param>
        /// <response code="200">OK,when AutomationExecutionLog is softdeleted,( isDeleted flag is set to true in DB) </response>
        /// <response code="400">BadRequest,If AutomationExecutionLog Id is null or empty Guid</response>
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
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of AutomationExecutionLog.
        /// </summary>
        /// <param name="id">AutomationExecutionLog identifier</param>
        /// <param name="request">Value of the AutomationExecutionLog to be updated.</param>
        /// <response code="200">OK,If update of AutomationExecutionLog is successful. </response>
        /// <response code="400">BadRequest,if the Id is null or Id's dont match.</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">Unprocessable entity,validation error</response>
        /// <returns>Ok response, if the partial AutomationExecutionLog values has been updated</returns>

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<AutomationExecutionLog> request)
        {
            return await base.PatchEntity(id, request);
        }
    }
}