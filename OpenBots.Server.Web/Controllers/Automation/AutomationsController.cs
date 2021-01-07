using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for Studio automations
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AutomationsController : EntityController<Automation>
    {
        private readonly IAutomationManager manager;
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBinaryObjectRepository binaryObjectRepo;
        private readonly IAutomationVersionRepository automationVersionRepo;
        private readonly StorageContext dbContext;
        IWebhookPublisher webhookPublisher;

        /// <summary>
        /// Automation Controller constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="manager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="binaryObjectRepo"></param>
        /// <param name="configuration"></param>
        public AutomationsController(
            IAutomationRepository repository,
            IAutomationManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectRepository binaryObjectRepo,
            IBinaryObjectManager binaryObjectManager,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            IAutomationVersionRepository automationVersionRepo,
            StorageContext dbContext) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            this.binaryObjectRepo = binaryObjectRepo;
            this.binaryObjectManager = binaryObjectManager;
            this.webhookPublisher = webhookPublisher;
            this.automationVersionRepo = automationVersionRepo;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Provides a list of all automations
        /// </summary>
        /// <response code="200">Ok, a paginated list of all automations</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all automations</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Automation>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<Automation> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a view model list of all automations and corresponding automation version information
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all automationes</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all automationes</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllAutomationsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AllAutomationsViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            ODataHelper<AllAutomationsViewModel> oData = new ODataHelper<AllAutomationsViewModel>();

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
                newNode = new OrderByNode<AllAutomationsViewModel>();

            Predicate<AllAutomationsViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<AllAutomationsViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return manager.GetAutomationsAndAutomationVersions(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
        }

        /// <summary>
        /// Gets count of automations in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all automations</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all automations</returns>
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
        /// Get automation by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a automation exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if automation id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no automation exists for the given automation id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Automation entity</returns>
        [HttpGet("{id}", Name = "GetAutomation")]
        [ProducesResponseType(typeof(PaginatedList<Automation>), StatusCodes.Status200OK)]
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
        /// Provides an automation's view model details for a particular automation id
        /// </summary>
        /// <param name="id">Automation id</param>
        /// <response code="200">Ok, if a automation exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if automation id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no automation exists for the given automation id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Automation view model details for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(AutomationViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<AutomationViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    AutomationViewModel view = okResult.Value as AutomationViewModel;
                    view = manager.GetAutomationView(view, id);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new automation entity
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new automation created and returned</response>
        /// <response code="400">Bad request, when the automation value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created automation details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Automation), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] AutomationViewModel request)
        {
            try
            {
                Guid versionId = Guid.NewGuid();
                var automation = new Automation()
                {
                    Name = request.Name,
                    AutomationEngine = request.AutomationEngine,
                    Id = request.Id
                };

                var response = await base.PostEntity(automation);
                manager.AddAutomationVersion(request);

                await webhookPublisher.PublishAsync("Automations.NewAutomationCreated", automation.Id.ToString(), automation.Name).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new binary object and upload automation file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <response code="200">Ok, automation updated and returned</response>
        /// <response code="400">Bad request, when the automation value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly updated automation details</returns>
        [HttpPost("{id}/upload")]
        [ProducesResponseType(typeof(Automation), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string id, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    ModelState.AddModelError("Save", "No automation uploaded");
                    return BadRequest(ModelState);
                }

                long size = file.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("Automation Upload", $"File size of automation {file.FileName} cannot be 0");
                    return BadRequest(ModelState);
                }

                var automation = repository.GetOne(Guid.Parse(id));
                string organizationId = binaryObjectManager.GetOrganizationId();
                string apiComponent = "AutomationAPI";

                BinaryObject binaryObject = new BinaryObject();
                binaryObject.Name = file.FileName;
                binaryObject.Folder = apiComponent;
                binaryObject.CreatedOn = DateTime.UtcNow;
                binaryObject.CreatedBy = applicationUser?.UserName;
                binaryObject.CorrelationEntityId = automation.Id;

                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                binaryObjectRepo.Add(binaryObject);

                automation.BinaryObjectId = (Guid)binaryObject.Id;
                automation.OriginalPackageName = file.FileName;
                repository.Update(automation);

                await webhookPublisher.PublishAsync("Files.NewFileCreated", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);
                await webhookPublisher.PublishAsync("Automations.AutomationUpdated", automation.Id.ToString(), automation.Name).ConfigureAwait(false);
                return Ok(automation);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update automation with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a automation, when automation id and the new details of automation are given
        /// </remarks>
        /// <param name="id">Automation id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Automation details to be updated</param>
        /// <response code="200">Ok, if the automation details for the given automation id have been updated</response>
        /// <response code="400">Bad request, if the automation id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value details</returns>
        [HttpPost("{id}/update")]
        [ProducesResponseType(typeof(Automation), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update(string id, [FromForm] AutomationViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingAutomation = repository.GetOne(entityId);
            if (existingAutomation == null) return NotFound();

            if (request.File == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            long size = request.File.Length;
            if (size <= 0)
            {
                ModelState.AddModelError("Automation Upload", $"File size of automation {request.File.FileName} cannot be 0");
                return BadRequest(ModelState);
            }

            string binaryObjectId = existingAutomation.BinaryObjectId.ToString();
            var binaryObject = binaryObjectRepo.GetOne(Guid.Parse(binaryObjectId));
            string organizationId = binaryObject.OrganizationId.ToString();

            if (!string.IsNullOrEmpty(organizationId))
                organizationId = manager.GetOrganizationId().ToString();

            try
            {
                BinaryObject newBinaryObject = new BinaryObject();
                if (existingAutomation.BinaryObjectId != Guid.Empty && size > 0)
                {
                    string apiComponent = "AutomationAPI";
                    //Update file in OpenBots.Server.Web using relative directory
                    newBinaryObject.Id = Guid.NewGuid();
                    newBinaryObject.Name = request.File.FileName;
                    newBinaryObject.Folder = apiComponent;
                    newBinaryObject.StoragePath = Path.Combine("BinaryObjects", organizationId, apiComponent, newBinaryObject.Id.ToString());
                    newBinaryObject.CreatedBy = applicationUser?.UserName;
                    newBinaryObject.CreatedOn = DateTime.UtcNow;
                    newBinaryObject.CorrelationEntityId = request.Id;
                    binaryObjectRepo.Add(newBinaryObject);
                    binaryObjectManager.Upload(request.File, organizationId, apiComponent, newBinaryObject.Id.ToString());
                    binaryObjectManager.SaveEntity(request.File, newBinaryObject.StoragePath, newBinaryObject, apiComponent, organizationId);
                    binaryObjectRepo.Update(binaryObject);
                }

                //Update automation (Create new automation and automation version entities)
                Automation response = existingAutomation;
                AutomationVersion automationVersion = automationVersionRepo.Find(null, q => q.AutomationId == response.Id).Items?.FirstOrDefault();
                if (existingAutomation.Name.Trim().ToLower() != request.Name.Trim().ToLower() || automationVersion.Status.Trim().ToLower() != request.Status?.Trim().ToLower()) 
                {
                    existingAutomation.BinaryObjectId = (Guid)newBinaryObject.Id;
                    existingAutomation.OriginalPackageName = request.File.FileName;
                    existingAutomation.AutomationEngine = request.AutomationEngine;
                    automationVersion.Status = request.Status;
                    response = manager.UpdateAutomation(existingAutomation, request);
                }

                await webhookPublisher.PublishAsync("Files.NewFileCreated", newBinaryObject.Id.ToString(), newBinaryObject.Name).ConfigureAwait(false);
                await webhookPublisher.PublishAsync("Automations.AutomationUpdated", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update a Automation 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a automation, when automation id and the new details of automation are given
        /// </remarks>
        /// <param name="id">Automation id, produces bad request if id is null or ids don't match</param>
        /// <param name="value">Automation details to be updated</param>
        /// <response code="200">Ok, if the automation details for the given automation id have been updated</response>
        /// <response code="400">Bad request, if the automation id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] AutomationViewModel value)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingAutomation = repository.GetOne(entityId);
                if (existingAutomation == null) return NotFound();

                existingAutomation.Name = value.Name;
                existingAutomation.AutomationEngine = value.AutomationEngine;

                var automationVersion = automationVersionRepo.Find(null, q => q.AutomationId == existingAutomation.Id).Items?.FirstOrDefault();
                if (!string.IsNullOrEmpty(automationVersion.Status))
                {
                    // Determine a way to check if previous value was not published before setting published properties
                    automationVersion.Status = value.Status;
                    if (automationVersion.Status == "Published")
                    {
                        automationVersion.PublishedBy = applicationUser?.Email;
                        automationVersion.PublishedOnUTC = DateTime.UtcNow;
                    }
                    automationVersionRepo.Update(automationVersion);
                }
                await webhookPublisher.PublishAsync("Automations.AutomationUpdated", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingAutomation);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Automation", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of an automation
        /// </summary>
        /// <param name="id">Automation identifier</param>
        /// <param name="request">Value of the automation to be updated</param>
        /// <response code="200">Ok, if update of automation is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity ,validation error</response>
        /// <returns>Ok response, if the partial automation values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Automation> request)
        {
            var existingAutomation = repository.GetOne(Guid.Parse(id));
            if (existingAutomation == null) return NotFound();

            await webhookPublisher.PublishAsync("Automations.AutomationUpdated", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Export/download a automation
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a automation exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if automation id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no automation exists for the given automation id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloaded automation file</returns>        
        [HttpGet("{id}/Export", Name = "ExportAutomation")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Export(string id)
        {
            try
            {
                Guid automationId;
                Guid.TryParse(id, out automationId);

                Automation automation = repository.GetOne(automationId);
               
                if (automation == null || automation.BinaryObjectId == null || automation.BinaryObjectId == Guid.Empty)
                {
                    ModelState.AddModelError("Automation Export", "No automation or automation file found");
                    return BadRequest(ModelState);
                }

                var fileObject = manager.Export(automation.BinaryObjectId.ToString());
                return File(fileObject?.Result?.BlobStream, fileObject?.Result?.ContentType, fileObject?.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete automation with a specified id from list of automationes
        /// </summary>
        /// <param name="id">Automation id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when automation is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if automation id is null or empty Guid</response>
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
                // Remove Automation
                Guid automationId = Guid.Parse(id);
                var existingAutomation = repository.GetOne(automationId);
                if (existingAutomation == null) return NotFound();

                await webhookPublisher.PublishAsync("Automations.AutomationDeleted", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                bool response = manager.DeleteAutomation(automationId);

                if (response)
                    return Ok();
                else
                {
                    ModelState.AddModelError("Automation Delete", "An error occured while deleting an automation");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all automationes
        /// </summary>
        /// <response code="200">Ok, a lookup list of all automationes</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Lookup list of all automationes</returns>
        [HttpGet("GetLookup")]
        [ProducesResponseType(typeof(List<JobAutomationLookup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public List<JobAutomationLookup> GetLookup()
        {
            var automationList = repository.Find(null, x => x.IsDeleted == false);
            var automationLookup = from p in automationList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                join v in dbContext.AutomationVersions on p.Id equals v.AutomationId into table1
                                from v in table1.DefaultIfEmpty()
                                select new JobAutomationLookup
                                {
                                    AutomationId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                    AutomationName = p?.Name,
                                    AutomationNameWithVersion = string.Format("{0} (v{1})", p?.Name.Trim(), v?.VersionNumber) 
                                };

            return automationLookup.ToList();
        }
    }
}
