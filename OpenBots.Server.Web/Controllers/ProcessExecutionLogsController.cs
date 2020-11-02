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
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ProcessExecutionLogsController : EntityController<ProcessExecutionLog>
    {
        readonly IAgentRepository agentRepository;
        IProcessExecutionLogManager processExecutionLogManager;
        public ProcessExecutionLogsController(
            IProcessExecutionLogRepository repository,
            IAgentRepository agentRepository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IProcessExecutionLogManager processExecutionLogManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.processExecutionLogManager = processExecutionLogManager;
            this.processExecutionLogManager.SetContext(base.SecurityContext);
            this.agentRepository = agentRepository;
        }

        /// <summary>
        /// Provides a list of all ProcessExecutionLogs
        /// </summary>
        /// <response code="200">OK,a Paginated list of all ProcessExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Paginated list of all ProcessExecutionLogs </returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ProcessExecutionLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<ProcessExecutionLog> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a viewmodel list of all ProcessExecutionLogs
        /// </summary>
        /// <response code="200">OK,a Paginated list of all ProcessExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Paginated list of all ProcessExecutionLogs </returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<ProcessExecutionViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<ProcessExecutionViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {

            ODataHelper<ProcessExecutionViewModel> oData = new ODataHelper<ProcessExecutionViewModel>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;
            var newNode = oData.ParseOrderByQuerry(queryString);
            if (newNode == null)
                newNode = new OrderByNode<ProcessExecutionViewModel>();

            Predicate<ProcessExecutionViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<ProcessExecutionViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return processExecutionLogManager.GetProcessAndAgentNames(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
        }

        /// <summary>
        /// Provides a Count of ProcessExecutionLogs 
        /// </summary>
        /// <response code="200">OK, total count of ProcessExecutionLogs</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response>        
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>Int contating the total number of ProcessExecutionLogs </returns>
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
        /// Provides a ProcessExecutionLog's details for a particular ProcessExecutionLog Id.
        /// </summary>
        /// <param name="id">ProcessExecutionLog id</param>
        /// <response code="200">OK, If a ProcessExecutionLog exists with the given Id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">BadRequest,If ProcessExecutionLog ID is not in the proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound, when no ProcessExecutionLog exists for the given ProcessExecutionLog ID</response>
        /// <returns>ProcessExecutionLog details for the given ID</returns>
        [HttpGet("{id}", Name = "GetProcessExecutionLog")]
        [ProducesResponseType(typeof(ProcessExecutionLog), StatusCodes.Status200OK)]
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
        /// Provides a processExecution's details for a particular processExecution Id.
        /// </summary>
        /// <param name="id">processExecution id</param>
        /// <response code="200">OK, If a processExecution exists with the given Id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">BadRequest,If processExecution ID is not in the proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">NotFound, when no processExecution exists for the given processExecution ID</response>
        /// <returns>processExecution details for the given ID</returns>
        [HttpGet("View/{id}")]
        [ProducesResponseType(typeof(ProcessExecutionViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<ProcessExecutionViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    ProcessExecutionViewModel view = okResult.Value as ProcessExecutionViewModel;
                    view = processExecutionLogManager.GetExecutionView(view);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new ProcessExecutionLog to the existing ProcessExecutionLogs
        /// </summary>
        /// <remarks>
        /// Adds the ProcessExecutionLog with unique ProcessExecutionLog Id to the existing ProcessExecutionLogs
        /// </remarks>
        /// <param name="value"></param>
        /// <response code="200">OK,new ProcessExecutionLog created and returned</response>
        /// <response code="400">BadRequest,When the ProcessExecutionLog value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict,concurrency error</response> 
        /// <response code="422">UnprocessabileEntity,when a duplicate record is being entered.</response>
        /// <returns> newly created unique ProcessExecutionLog Id with route name </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProcessExecutionLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] ProcessExecutionLog request)
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
        /// Allows Agent to add a new ProcessExecutionLog to the existing ProcessExecutionLogs
        /// </summary>
        /// <remarks>
        /// Agent is able to Add the ProcessExecutionLog if the Agent is Connected
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">OK,new ProcessExecutionLog created and returned</response>
        /// <response code="400">BadRequest,When the ProcessExecutionLog value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict,concurrency error</response> 
        /// <response code="422">UnprocessabileEntity,when a duplicate record is being entered.</response>
        /// <returns> newly created unique ProcessExecutionLog Id with route name </returns>
        [AllowAnonymous]
        [HttpPost("StartProcess")]
        [ProducesResponseType(typeof(ProcessExecutionLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> StartProcess([FromBody] ProcessExecutionLog request)
        {
            try
            {
                var agent = agentRepository.Find(null, a=> a.Id == request.AgentID)?.Items?.FirstOrDefault();

                if (agent == null)
                {
                    ModelState.AddModelError("StartProcess", "Agent not found");
                    return NotFound(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("StartProcess", "Agent is not connected");
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
        /// Updates a ProcessExecutionLog 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a ProcessExecutionLog, when ProcessExecutionLog id and the new details of ProcessExecutionLog are given
        /// </remarks>
        /// <param name="id">ProcessExecutionLog Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">ProcessExecutionLog details to be updated</param>
        /// <response code="200">OK, If the ProcessExecutionLog details for the given ProcessExecutionLog Id has been updated.</response>
        /// <response code="400">BadRequest,if the ProcessExecutionLog Id is null or Id's don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] ProcessExecutionLog request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingProcessExecutionLog = repository.GetOne(entityId);
                if (existingProcessExecutionLog == null) return NotFound();

                existingProcessExecutionLog.JobID = request.JobID;
                existingProcessExecutionLog.ProcessID = request.ProcessID;
                existingProcessExecutionLog.AgentID = request.AgentID;
                existingProcessExecutionLog.StartedOn = request.StartedOn;
                existingProcessExecutionLog.CompletedOn = request.CompletedOn;
                existingProcessExecutionLog.Trigger = request.Trigger;
                existingProcessExecutionLog.TriggerDetails = request.TriggerDetails;
                existingProcessExecutionLog.Status = request.Status;
                existingProcessExecutionLog.HasErrors = request.HasErrors;
                existingProcessExecutionLog.ErrorMessage = request.ErrorMessage;
                existingProcessExecutionLog.ErrorDetails = request.ErrorDetails;

                return await base.PutEntity(id, existingProcessExecutionLog);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ProcessExecutionLog", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Agent is able to update a ProcessExecutionLog End status
        /// </summary>
        /// <remarks>
        /// Provides an action to update a ProcessExecutionLog, when ProcessExecutionLog id and the new details of ProcessExecutionLog are given
        /// </remarks>
        /// <param name="id">ProcessExecutionLog Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">ProcessExecutionLog details to be updated</param>
        /// <response code="200">OK, If the ProcessExecutionLog details for the given ProcessExecutionLog Id has been updated.</response>
        /// <response code="400">BadRequest,if the ProcessExecutionLog Id is null or Id's don't match</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">UnprocessableEntity</response>
        /// <returns>OK response with the updated value</returns>
        [AllowAnonymous]
        [HttpPut("{id}/EndProcess")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> EndProcess(string id, [FromBody] ProcessExecutionLog request)
        {
            try
            {
                var agent = agentRepository.Find(null, a => a.Id == request.AgentID)?.Items?.FirstOrDefault();

                if (agent == null)
                {
                    ModelState.AddModelError("StartProcess", "Agent not found");
                    return NotFound(ModelState);
                }
                if (agent.IsConnected == false)
                {
                    ModelState.AddModelError("StartProcess", "Agent is not connected");
                    return BadRequest(ModelState);
                }
                Guid entityId = new Guid(id);

                var existingProcessExecutionLog = repository.GetOne(entityId);
                if (existingProcessExecutionLog == null) return NotFound();

                existingProcessExecutionLog.JobID = request.JobID;
                existingProcessExecutionLog.ProcessID = request.ProcessID;
                existingProcessExecutionLog.AgentID = request.AgentID;
                existingProcessExecutionLog.StartedOn = request.StartedOn;
                existingProcessExecutionLog.CompletedOn = request.CompletedOn;
                existingProcessExecutionLog.Trigger = request.Trigger;
                existingProcessExecutionLog.TriggerDetails = request.TriggerDetails;
                existingProcessExecutionLog.Status = request.Status;
                existingProcessExecutionLog.HasErrors = request.HasErrors;
                existingProcessExecutionLog.ErrorMessage = request.ErrorMessage;
                existingProcessExecutionLog.ErrorDetails = request.ErrorDetails;

                return await base.PutEntity(id, existingProcessExecutionLog);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ProcessExecutionLog", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes a ProcessExecutionLog with a specified id from the ProcessExecutionLog.
        /// </summary>
        /// <param name="id">ProcessExecutionLog ID to be deleted- throws BadRequest if null or empty Guid/</param>
        /// <response code="200">OK,when ProcessExecutionLog is softdeleted,( isDeleted flag is set to true in DB) </response>
        /// <response code="400">BadRequest,If ProcessExecutionLog Id is null or empty Guid</response>
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
        /// Updates partial details of ProcessExecutionLog.
        /// </summary>
        /// <param name="id">ProcessExecutionLog identifier</param>
        /// <param name="value">Value of the ProcessExecutionLog to be updated.</param>
        /// <response code="200">OK,If update of ProcessExecutionLog is successful. </response>
        /// <response code="400">BadRequest,if the Id is null or Id's dont match.</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">Unprocessable entity,validation error</response>
        /// <returns>Ok response, if the partial ProcessExecutionLog values has been updated</returns>

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<ProcessExecutionLog> request)
        {
            return await base.PatchEntity(id, request);
        }
    }
}