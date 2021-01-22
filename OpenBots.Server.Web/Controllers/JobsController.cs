using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for Jobs
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : EntityController<Job>
    {
        readonly IJobManager jobManager;
        readonly IJobParameterRepository jobParameterRepo;
        readonly IAutomationRepository automationRepo;
        readonly IJobCheckpointRepository jobCheckpointRepo;
        private IHubContext<NotificationHub> _hub;
        private IAutomationVersionRepository automationVersionRepo;
        private readonly IWebhookPublisher webhookPublisher;
        private readonly IJobRepository repository;

        /// <summary>
        /// JobsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="jobManager"></param>
        /// <param name="hub"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="jobCheckpointRepository"></param>
        /// <param name="jobParameterRepository"></param>
        /// <param name="automationRepository"></param>
        /// <param name="automationVersionRepo"></param>
        /// <param name="webhookPublisher"></param>
        public JobsController(
            IJobRepository repository,
            IAutomationRepository automationRepository,
            IJobParameterRepository jobParameterRepository,
            IJobCheckpointRepository jobCheckpointRepository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IJobManager jobManager,
            IHubContext<NotificationHub> hub,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IAutomationVersionRepository automationVersionRepo,
            IWebhookPublisher webhookPublisher) : base(repository, userManager, httpContextAccessor,
                membershipManager, configuration)
        {
            this.jobManager = jobManager;
            this.jobParameterRepo = jobParameterRepository;
            this.automationRepo = automationRepository;
            this.jobCheckpointRepo = jobCheckpointRepository;
            this.jobManager.SetContext(base.SecurityContext);
            this.repository = repository;
            _hub = hub;
            this.automationVersionRepo = automationVersionRepo;
            this.webhookPublisher = webhookPublisher;
        }

        /// <summary>
        /// Provides a list of all jobs
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all jobs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all jobs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Job>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<Job> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a view model list of all jobs
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all jobs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all jobs</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllJobsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AllJobsViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            ODataHelper<AllJobsViewModel> oDataHelper = new ODataHelper<AllJobsViewModel>();

            var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

            return jobManager.GetJobAgentsandAutomations(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);
        }

        /// <summary>
        /// Provides a count of jobs 
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, total count of jobs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <response code="404">Not found</response>
        /// <returns>Total count of jobs</returns>
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
        /// Provides a count of jobs by job status
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, list of job status counts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>     
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>List of status and count of jobs in a key value pair list</returns>
        [HttpGet("CountByStatus")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public Dictionary<string, int> CountByStatus(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            var result =  base.GetMany();
            var grouping = result.Items.GroupBy(job => job.JobStatus);
            Dictionary<string, int> count = new Dictionary<string, int>();

            count["Total Jobs"] = result.Items.Count();

            foreach (var status in grouping)
            {
                count[status.Key.ToString()] = status.Count();
            }
            return count;
        }

        /// <summary>
        /// Provides a lookup list of all job agents and automations
        /// </summary>
        /// <response code="200">Ok, a list of all jobs lookup</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all jobs lookup</returns>
        [HttpGet("JobAgentsLookup")]
        [ProducesResponseType(typeof(JobsLookupViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public JobsLookupViewModel GetJobAgentsLookup()
        {
            return jobManager.GetJobAgentsLookup();
        }

        /// <summary>
        /// Provides a job's details for a particular job id
        /// </summary>
        /// <param name="id">Job id</param>
        /// <response code="200">Ok,if a job exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if job id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no job exists for the given job id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Job details for the given id</returns>
        [HttpGet("{id}", Name = "GetJob")]
        [ProducesResponseType(typeof(Job), StatusCodes.Status200OK)]
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
        /// Provides a job's view model details for a particular job id
        /// </summary>
        /// <param name="id">Job id</param>
        /// <response code="200">Ok, if a job exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if job id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no job exists for the given job id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Job view model details for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(JobViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<JobViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    JobViewModel view = okResult.Value as JobViewModel;
                    view = jobManager.GetJobView(view);
                }
                
                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets the next available job for the provided agent id
        /// </summary>
        /// <param name="agentId">Agent id</param>
        /// <response code="200">Ok, if ajJob exists for the given agent id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Returns the oldest available job and sets its status to 'Assigned'</returns>
        [HttpGet("Next")]
        [ProducesResponseType(typeof(NextJobViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Next([FromQuery] string agentId)
        {
            try
            {
                if (agentId == null)
                {
                    ModelState.AddModelError("Next", "No Agent ID was passed");
                    return BadRequest(ModelState);
                }

                bool isValid = Guid.TryParse(agentId, out Guid agentGuid);
                if (!isValid)
                {
                    ModelState.AddModelError("Next", "Agent ID is not a valid GUID ");
                    return BadRequest(ModelState);
                }

                JsonPatchDocument<Job> statusPatch = new JsonPatchDocument<Job>();
                statusPatch.Replace(j => j.JobStatus, JobStatusType.Assigned);
                statusPatch.Replace(j => j.DequeueTime, DateTime.UtcNow);

                NextJobViewModel nextJob = jobManager.GetNextJob(agentGuid);

                return Ok(nextJob);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Exports all jobs into a downloadable file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="fileType">Specifies the file type to be downloaded: csv, zip, or json</param>
        /// <response code="200">Ok, if a job exists with the given filters</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns> Downloadable file</returns>
        [HttpGet("export/{filetype?}")]
        [Produces("text/csv", "application/zip", "application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<object> Export(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0, string fileType = "")
        {
            try
            {
                //determine top value
                int maxExport = int.Parse(config["App:MaxExportRecords"]);
                top = top > maxExport | top == 0 ? maxExport : top; //if $top is greater than max or equal to 0 use max export value
                ODataHelper<Job> oData = new ODataHelper<Job>();
                string queryString = HttpContext.Request.QueryString.Value;

                oData.Parse(queryString);
                oData.Top = top;

                var jobsJson = base.GetMany(oData: oData);
                string csvString = jobManager.GetCsv(jobsJson.Items.ToArray());
                var csvFile = File(new System.Text.UTF8Encoding().GetBytes(csvString), "text/csv", "Jobs.csv");

                switch (fileType.ToLower())
                {
                    case "csv":
                        return csvFile;

                    case "zip":
                        var zippedFile = jobManager.ZipCsv(csvFile);
                        const string contentType = "application/zip";
                        HttpContext.Response.ContentType = contentType;
                        var zipFile = new FileContentResult(zippedFile.ToArray(), contentType)
                        {
                            FileDownloadName = "Jobs.zip"
                        };

                        return zipFile;

                    case "json":
                        return jobsJson;
                }
                return csvFile;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Export", ex.Message);
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new job to the existing jobs
        /// </summary>
        /// <remarks>
        /// Adds the job with unique job id to the existing jobs
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new job created and returned</response>
        /// <response code="400">Bad request, when the job value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created job details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Job), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] CreateJobViewModel request)
        {
            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            Guid entityId = Guid.NewGuid();
            if (request.Id == null || !request.Id.HasValue || request.Id.Equals(Guid.Empty))
                request.Id = entityId;
            try
            {
                Job job = request.Map(request); //assign request to job entity
                Automation automation = automationRepo.GetOne(job.AutomationId ?? Guid.Empty);
                
                if (automation == null) //no automation was found
                {
                    ModelState.AddModelError("Save", "No automation was found for the specified automation id");
                    return NotFound(ModelState);
                }
                AutomationVersion automationVersion = automationVersionRepo.Find(null, q => q.AutomationId == automation.Id).Items?.FirstOrDefault();

                job.AutomationVersion = automationVersion.VersionNumber;
                job.AutomationVersionId = automationVersion.Id;

                foreach (var parameter in request.JobParameters ?? Enumerable.Empty<JobParameter>())
                {
                    parameter.JobId = entityId;
                    parameter.CreatedBy = applicationUser?.UserName;
                    parameter.CreatedOn = DateTime.UtcNow;
                    parameter.Id = Guid.NewGuid();
                    jobParameterRepo.Add(parameter);
                }

                //send SignalR notification to all connected clients 
                await _hub.Clients.All.SendAsync("botnewjobnotification", request.AgentId.ToString());
                await _hub.Clients.All.SendAsync("sendjobnotification", "New Job added.");
                await _hub.Clients.All.SendAsync("broadcastnewjobs", Tuple.Create(request.Id,request.AgentId,request.AutomationId));
                await webhookPublisher.PublishAsync("Jobs.NewJobCreated", job.Id.ToString()).ConfigureAwait(false);


                return await base.PostEntity(job);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a job 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a job, when job id and the new details of a job are given
        /// </remarks>
        /// <param name="id">Job id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Job details to be updated</param>
        /// <response code="200">Ok, if the job details for the given job id has been updated</response>
        /// <response code="400">Bad request, if the job id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>k response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] CreateJobViewModel request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingJob = repository.GetOne(entityId);
                if (existingJob == null) return NotFound("Unable to find a Job for the specified ID");

                Automation automation = automationRepo.GetOne(existingJob.AutomationId ?? Guid.Empty);
                if (automation == null) //no automation was found
                {
                    ModelState.AddModelError("Save", "No automation was found for the specified automation ID");
                    return NotFound(ModelState);
                }

                AutomationVersion automationVersion = automationVersionRepo.Find(null, q => q.AutomationId == automation.Id).Items?.FirstOrDefault();
                existingJob.AutomationVersion = automationVersion.VersionNumber;
                existingJob.AutomationVersionId = automationVersion.Id;

                existingJob.AgentId = request.AgentId;
                existingJob.StartTime = request.StartTime;
                existingJob.EndTime = request.EndTime;
                existingJob.ExecutionTimeInMinutes = (existingJob.EndTime.Value - existingJob.StartTime).Value.TotalMinutes;
                existingJob.DequeueTime = request.DequeueTime;
                existingJob.AutomationId = request.AutomationId;
                existingJob.JobStatus = request.JobStatus;
                existingJob.Message = request.Message;
                existingJob.IsSuccessful = request.IsSuccessful;

                var response = await base.PutEntity(id, existingJob);

                jobManager.DeleteExistingParameters(entityId);

                var set = new HashSet<string>();
                foreach (var parameter in request.JobParameters ?? Enumerable.Empty<JobParameter>())
                {
                    if (!set.Add(parameter.Name)) 
                    {
                        ModelState.AddModelError("JobParameter", "JobParameter Name Already Exists");
                        return BadRequest(ModelState);
                    }
                    parameter.JobId = entityId;
                    parameter.CreatedBy = applicationUser?.UserName;
                    parameter.CreatedOn = DateTime.UtcNow;
                    parameter.Id = Guid.NewGuid();
                    jobParameterRepo.Add(parameter);
                }

                if (request.EndTime != null)
                {
                    jobManager.UpdateAutomationAverages(existingJob.Id);
                }

                //send SignalR notification to all connected clients 
                await webhookPublisher.PublishAsync("Jobs.JobUpdated", existingJob.Id.ToString()).ConfigureAwait(false);
                await _hub.Clients.All.SendAsync("sendjobnotification", string.Format("Job id {0} updated.", existingJob.Id));

                return response;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Job", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates a job with the specified status
        /// </summary>
        /// <remarks>
        /// Provides an action to update a job status, when job id and the new details of job are given
        /// </remarks>
        /// <param name="id">Job id, produces bad request if id is null or ids don't match</param>
        /// <param name="status">Status value for the specified job id</param>
        /// <param name="agentId">Id of agent that is updating job status</param>
        /// <param name="jobErrors">Job error details to be updated</param>
        /// <response code="200">Ok, if the job details for the given job id has been updated</response>
        /// <response code="400">Bad request, if the job id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found, when no job exists for the given agent id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}/Status/{status}")]
        [ProducesResponseType(typeof(Job), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ChangeStatus(string id, JobStatusType status, [BindRequired, FromQuery] string agentId, [FromBody] JobErrorViewModel jobErrors)
        {
            try
            {
                if (id == null)
                {
                    ModelState.AddModelError("ChangeStatus", "No Job ID was passed");
                    return BadRequest(ModelState);
                }

                bool isValid = Guid.TryParse(id, out Guid agentGuid);
                if (!isValid)
                {
                    ModelState.AddModelError("ChangeStatus", "Job ID is not a valid GUID ");
                    return BadRequest(ModelState);
                }
                if (status == null)
                {
                    ModelState.AddModelError("ChangeStatus", "No status was provided");
                    return BadRequest(ModelState);
                }

                Guid entityId = new Guid(id);

                var existingJob = repository.GetOne(entityId);
                if (existingJob == null) return NotFound("Unable to find a Job for the specified ID");

                if (existingJob.AgentId.ToString() != agentId)
                {
                    return UnprocessableEntity("The provided Agent ID does not match the Job's Agent ID");
                }

                switch (status)
                {
                    case JobStatusType.Completed:
                        existingJob.IsSuccessful = true;
                        break;
                    case JobStatusType.Failed:
                        existingJob.IsSuccessful = false;
                        break;
                }

                existingJob.JobStatus = status;
                existingJob.ErrorReason = string.IsNullOrEmpty(jobErrors.ErrorReason) ? existingJob.ErrorReason : jobErrors.ErrorReason;
                existingJob.ErrorCode = string.IsNullOrEmpty(jobErrors.ErrorCode) ? existingJob.ErrorCode : jobErrors.ErrorReason;
                existingJob.SerializedErrorString = string.IsNullOrEmpty(jobErrors.SerializedErrorString) ? existingJob.SerializedErrorString : jobErrors.ErrorReason;

                var response = await base.PutEntity(id, existingJob);
                //send SignalR notification to all connected clients 
                await _hub.Clients.All.SendAsync("sendjobnotification", string.Format("Job id {0} updated.", existingJob.Id));
                await webhookPublisher.PublishAsync("Jobs.JobUpdated", existingJob.Id.ToString()).ConfigureAwait(false);

                return response;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Job", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes a job with a specified id from the job
        /// </summary>
        /// <param name="id">Job id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when job is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if job id is null or empty Guid</response>
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
            Guid jobId = new Guid(id);
            var existingJob = repository.GetOne(jobId);

            if (existingJob == null)
            {
                ModelState.AddModelError("Job", "Job cannot be found or does not exist.");
                return NotFound(ModelState);
            }

            var response = await base.DeleteEntity(id);
            jobManager.DeleteExistingParameters(jobId);
            jobManager.DeleteExistingCheckpoints(jobId);

            //send SignalR notification to all connected clients 
            await _hub.Clients.All.SendAsync("sendjobnotification", string.Format("Job id {0} deleted.", id));
            await webhookPublisher.PublishAsync("Jobs.JobDeleted", existingJob.Id.ToString()).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// Updates partial details of a job
        /// </summary>
        /// <param name="id">Job identifier</param>
        /// <param name="request">Value of the job to be updated</param>
        /// <response code="200">OK,If update of Job is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial job values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Job> request)
        {
            try
            {
                Guid jobId = new Guid(id);
                bool endTimeReported = false;
                var existingJob = repository.GetOne(jobId);

                if (existingJob == null)
                {
                    ModelState.AddModelError("Job", "Job cannot be found or does not exist.");
                    return NotFound(ModelState);
                }

                for (int i = 0; i < request.Operations.Count; i++)
                {
                    if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/endtime")
                    {
                        double executionTime = (DateTime.Parse(request.Operations[i].value.ToString()) - existingJob.StartTime).Value.TotalMinutes;
                        request.Replace(j => j.ExecutionTimeInMinutes, executionTime);
                        endTimeReported = true;
                    }
                }

                var response = await base.PatchEntity(id, request);

                if (endTimeReported)
                {
                    jobManager.UpdateAutomationAverages(existingJob.Id);
                }

                //send SignalR notification to all connected clients 
                await _hub.Clients.All.SendAsync("sendjobnotification", string.Format("Job id {0} updated.", id));
                await webhookPublisher.PublishAsync("Jobs.JobUpdated", existingJob.Id.ToString()).ConfigureAwait(false);

                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }

        }

        /// <summary>
        /// Adds checkpoint to the existing JobCheckPoints
        /// </summary>
        /// <remarks>
        /// Creates a new Job Checkpoint for the specified job id
        /// </remarks>
        /// <param name="jobId"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, new checkpoint created and returned</response>
        /// <response code="400">Bad request, when the job value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns> Newly created Checkpoint details</returns>
        [HttpPost("{JobId}/AddCheckpoint")]
        [ProducesResponseType(typeof(JobCheckpoint), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AddCheckpoint([FromBody] JobCheckpoint request, string jobId)
        {
            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            Guid entityId = Guid.NewGuid();
            if (request.Id == null || !request.Id.HasValue || request.Id.Equals(Guid.Empty))
                request.Id = entityId;

            Job job = repository.GetOne(new Guid(jobId));
            if (job == null)
            {
                return NotFound("The Job ID provided does not match any existing Jobs");
            }

            try
            {
                request.JobId = new Guid(jobId);
                request.CreatedBy = applicationUser?.UserName;
                request.CreatedOn = DateTime.UtcNow;
                jobCheckpointRepo.Add(request);
                var resultRoute = "GetJobCheckpoint";

                return CreatedAtRoute(resultRoute, new { id = request.Id.Value.ToString("b") }, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a checkpoint's view model details for a particular job id
        /// </summary>
        /// <param name="jobId">Job id</param>
        /// <response code="200">Ok, if a checkpoint exists for the given job id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if job id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no job exists for the given job id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>JobCheckpoint details for the given id</returns>
        [HttpGet("{JobId}/JobCheckpoints", Name = "GetJobCheckpoint")]
        [ProducesResponseType(typeof(PaginatedList<JobCheckpoint>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> JobCheckpoints(
            string jobId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            Job job = repository.GetOne(new Guid(jobId));
            if (job == null)
            {
                return NotFound("The Job ID provided does not match any existing Jobs");
            }

            ODataHelper<JobCheckpoint> oDataHelper = new ODataHelper<JobCheckpoint>();

            Guid parentguid = Guid.Empty;

            var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

            return Ok(jobCheckpointRepo.Find(parentguid, oData.Filter, oData.Sort, oData.SortDirection, oData.Skip,
                oData.Top).Items.Where(c=> c.JobId == new Guid(jobId)));
        }
    }
}