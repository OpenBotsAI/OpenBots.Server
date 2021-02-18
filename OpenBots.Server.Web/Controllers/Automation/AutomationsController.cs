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
        private readonly IAutomationManager _manager;
        private readonly StorageContext _dbContext;
        private readonly IWebhookPublisher _webhookPublisher;

        /// <summary>
        /// Automation Controller constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="manager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="webhookPublisher"></param>
        /// <param name="dbContext"></param>
        public AutomationsController(
            IAutomationRepository repository,
            IAutomationManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            StorageContext dbContext) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _manager = manager;
            _webhookPublisher = webhookPublisher;
            _dbContext = dbContext;
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
        public async Task<IActionResult> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                ODataHelper<AllAutomationsViewModel> oDataHelper = new ODataHelper<AllAutomationsViewModel>();
                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_manager.GetAutomationsAndAutomationVersions(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
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
        public async Task<IActionResult> GetCount(
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
                    view = _manager.GetAutomationView(view, id);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new automation entity and file
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new automation entity created and returned</response>
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
        public async Task<IActionResult> Post([FromForm] AutomationViewModel request)
        {
            try
            {
                var automation = _manager.AddAutomation(request);
                var response = await base.PostEntity(automation);

                await _webhookPublisher.PublishAsync("Automations.NewAutomationCreated", automation.Id.ToString(), automation.Name).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update automation with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an automation, when automation id and the new details of automation are given
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
            try
            {
                var automation = _manager.UpdateAutomationFile(id, request);
                var response = await base.PostEntity(automation);
                await _webhookPublisher.PublishAsync("Automations.NewAutomationCreated", automation.Id.ToString(), automation.Name).ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update automation entity
        /// </summary>
        /// <remarks>
        /// Provides an action to update an automation, when automation id and the new details of automation are given
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
        public async Task<IActionResult> Put(string id, [FromBody] AutomationViewModel request)
        {
            try
            {
                var existingAutomation = _manager.UpdateAutomation(id, request);
                await _webhookPublisher.PublishAsync("Automations.AutomationUpdated", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingAutomation);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
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
            try
            {
                var existingAutomation = repository.GetOne(Guid.Parse(id));
                if (existingAutomation == null) return NotFound();

                await _webhookPublisher.PublishAsync("Automations.AutomationUpdated", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Export/download an automation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="driveName"></param>
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
        public async Task<IActionResult> Export(string id, string driveName = null)
        {
            try
            {
                var fileObject = _manager.Export(id, driveName);
                return File(fileObject.Result?.Content, fileObject.Result?.ContentType, fileObject.Result?.Name);
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
        /// <param name="driveName"></param>
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
        public async Task<IActionResult> Delete(string id, string driveName)
        {
            try
            {
                //remove automation
                Guid automationId = Guid.Parse(id);
                var existingAutomation = repository.GetOne(automationId);
                if (existingAutomation == null) return NotFound();

                await _webhookPublisher.PublishAsync("Automations.AutomationDeleted", existingAutomation.Id.ToString(), existingAutomation.Name).ConfigureAwait(false);
                _manager.DeleteAutomation(existingAutomation, driveName);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all automations
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
        public async Task<IActionResult> GetLookup()
        {
            try
            {
                var automationList = repository.Find(null, x => x.IsDeleted == false);
                var automationLookup = from p in automationList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                       join v in _dbContext.AutomationVersions on p.Id equals v.AutomationId into table1
                                       from v in table1.DefaultIfEmpty()
                                       select new JobAutomationLookup
                                       {
                                           AutomationId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                           AutomationName = p?.Name,
                                           AutomationNameWithVersion = string.Format("{0} (v{1})", p?.Name.Trim(), v?.VersionNumber)
                                       };

                return Ok(automationLookup.ToList());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
