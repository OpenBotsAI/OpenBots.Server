using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;
using Hangfire;
using System.Text.Json;
using OpenBots.Server.Web.Hubs;
using Cronos;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for Schedules
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulesController : EntityController<Schedule>
    {
        private IScheduleManager manager;
        private IHubManager hubManager;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;

        /// <summary>
        /// SchedulesController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="manager"></param>
        /// <param name="backgroundJobClient"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="hubManager"></param>
        public SchedulesController(
            IScheduleRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IScheduleManager manager,
            IBackgroundJobClient backgroundJobClient,
            IHttpContextAccessor httpContextAccessor,
            IHubManager hubManager,
            IConfiguration configuration,
            IRecurringJobManager recurringJobManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            this.manager.SetContext(SecurityContext);
            this.backgroundJobClient = backgroundJobClient;
            this.hubManager = hubManager;
            this.recurringJobManager = recurringJobManager;
        }

        /// <summary>
        /// Provides a list of all schedules
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok,a paginated list of all schedules</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all schedules </returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ScheduleViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<ScheduleViewModel> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            ODataHelper<ScheduleViewModel> oData = new ODataHelper<ScheduleViewModel>();

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
                newNode = new OrderByNode<ScheduleViewModel>();

            Predicate<ScheduleViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<ScheduleViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return manager.GetScheduleAgentsandProcesses(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
        }

        /// <summary>
        /// Gets count of Schedules in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all schedules</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all schedules</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Provides schedule details for a particular schedule id
        /// </summary>
        /// <param name="id">Schedule id</param>
        /// <response code="200">Ok, if a schedule exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if schedule id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no schedule exists for the given schedule id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Schedule details for the given id</returns>
        [HttpGet("{id}", Name = "GetSchedule")]
        [ProducesResponseType(typeof(PaginatedList<Schedule>), StatusCodes.Status200OK)]
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
        /// Adds a new schedule to the existing schedules
        /// </summary>
        /// <remarks>
        /// Adds the schedule with unique schedule id to the existing schedules
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok ,new schedule created and returned</response>
        /// <response code="400">Bad request, when the schedule value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique schedule id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Schedule), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] ScheduleViewModel request)
        {
            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            //Validate the cron expression
            if (!string.IsNullOrWhiteSpace(request.CRONExpression))
            {
                try
                {
                    CronExpression expression = CronExpression.Parse(request.CRONExpression, CronFormat.Standard);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Save", string.Concat("Invalid cron expression, ", ex.Message));
                    return BadRequest(ModelState);
                }
            }

            Guid entityId = Guid.NewGuid();

            try
            {
                Schedule requestObj = new Schedule();
                requestObj.Id = entityId;
                requestObj.Name = request.Name;
                requestObj.AgentId = request.AgentId;
                requestObj.CRONExpression = request.CRONExpression;
                requestObj.LastExecution = request.LastExecution;
                requestObj.NextExecution = request.NextExecution;
                requestObj.IsDisabled = request.IsDisabled;
                requestObj.ProjectId = request.ProjectId;
                requestObj.StartingType = request.StartingType;
                requestObj.Status = request.Status;
                requestObj.ExpiryDate = request.ExpiryDate;
                requestObj.StartDate = request.StartDate;
                requestObj.ProcessId = request.ProcessId;

                var response = await base.PostEntity(requestObj);
                try
                {
                    recurringJobManager.RemoveIfExists(requestObj.Id?.ToString());

                    if (request.IsDisabled == false && !request.StartingType.ToLower().Equals("manual"))
                    {
                        var jsonScheduleObj = JsonSerializer.Serialize<Schedule>(requestObj);

                        backgroundJobClient.Schedule(() => hubManager.StartNewRecurringJob(jsonScheduleObj),
                                new DateTimeOffset(requestObj.StartDate.Value));
                    }
                }
                catch (Exception ex) { }

                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }


        /// <summary>
        /// Updates a schedule 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a schedule, when schedule id and the new details of the schedule are given
        /// </remarks>
        /// <param name="id">Schedule id,produces bad request if id is null or ids don't match</param>
        /// <param name="request">Schedule details to be updated</param>
        /// <response code="200">Ok, if the schedule details for the given schedule id have been updated</response>
        /// <response code="400">Bad request, if the schedule id is null or ids don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] ScheduleViewModel request)
        {
            try
            {
                //validate the cron expression
                if (!string.IsNullOrWhiteSpace(request.CRONExpression))
                {
                    try
                    {
                        CronExpression expression = CronExpression.Parse(request.CRONExpression, CronFormat.Standard);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Save", string.Concat("Invalid cron expression, ", ex.Message));
                        return BadRequest(ModelState);
                    }
                }

                Guid entityId = new Guid(id);

                var existingSchedule = repository.GetOne(entityId);
                if (existingSchedule == null) return NotFound();

                existingSchedule.Name = request.Name;
                existingSchedule.AgentId = request.AgentId;
                existingSchedule.CRONExpression = request.CRONExpression;
                existingSchedule.LastExecution = request.LastExecution;
                existingSchedule.NextExecution = request.NextExecution;
                existingSchedule.IsDisabled = request.IsDisabled;
                existingSchedule.ProjectId = request.ProjectId;
                existingSchedule.StartingType = request.StartingType;
                existingSchedule.Status = request.Status;
                existingSchedule.ExpiryDate = request.ExpiryDate;
                existingSchedule.StartDate = request.StartDate;
                existingSchedule.ProcessId = request.ProcessId;

                var response = await base.PutEntity(id, existingSchedule);
                try
                {
                    recurringJobManager.RemoveIfExists(existingSchedule.Id?.ToString());
                    
                    if (request.IsDisabled == false && !request.StartingType.ToLower().Equals("manual"))
                    {
                        var jsonScheduleObj = JsonSerializer.Serialize<Schedule>(existingSchedule);

                        backgroundJobClient.Schedule(() => hubManager.StartNewRecurringJob(jsonScheduleObj),
                               new DateTimeOffset(existingSchedule.StartDate.Value));
                    }
                }
                catch (Exception ex) { }

                return response;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Schedule", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes a schedule with a specified id from the schedules
        /// </summary>
        /// <param name="id">Schedule id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when schedule is soft deleted,(isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if schedule id is null or empty Guid</response>
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
                Guid entityId = new Guid(id);

                var existingSchedule = repository.GetOne(entityId);
                if (existingSchedule == null) return NotFound();

                recurringJobManager.RemoveIfExists(existingSchedule.Id.Value.ToString());

                return await base.DeleteEntity(id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Schedule", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of Schedule.
        /// </summary>
        /// <param name="id">Schedule identifier</param>
        /// <param name="request">Value of the Schedule to be updated.</param>
        /// <response code="200">OK,If update of Schedule is successful. </response>
        /// <response code="400">BadRequest,if the Id is null or Id's dont match.</response>
        /// <response code="403">Forbidden,unauthorized access</response>
        /// <response code="422">Unprocessable entity,validation error</response>
        /// <returns>Ok response, if the partial Schedule values has been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<Schedule> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// API to run a job now
        /// </summary>
        /// <param name="processId">Process id, against which job will be created</param>
        /// <param name="agentId">Agent id, against which job will be created</param>
        /// <response code="200">Ok, if the job enqueues successfully</response>
        /// <response code="400">Bad request, if the process id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response</returns>
        [HttpPost("Process/{processId}/RunNow")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RunNow(string processId, string agentId)
        {
            try
            {
                Guid ProcessID = new Guid(processId);
                Guid AgentID = new Guid(agentId);

                Schedule schedule = new Schedule();
                schedule.AgentId = AgentID;
                schedule.CRONExpression = "";
                schedule.LastExecution = DateTime.Now;
                schedule.NextExecution = DateTime.Now;
                schedule.IsDisabled = false;
                schedule.ProjectId = null;
                schedule.StartingType = "";
                schedule.Status = "New";
                schedule.ExpiryDate = DateTime.Now.AddDays(1);
                schedule.StartDate = DateTime.Now;
                schedule.ProcessId = ProcessID;
                
                var jsonScheduleObj = JsonSerializer.Serialize<Schedule>(schedule); 
                var jobId = BackgroundJob.Enqueue(() => hubManager.StartNewRecurringJob(jsonScheduleObj));

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("RunNow", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
