using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel.Lookup;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.WebHooksApi
{
    /// <summary>
    /// ReadOnlyController for IntegrationEventLog
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationEventLogsController : ReadOnlyEntityController<IntegrationEventLog>
    {
        private readonly IIntegrationEventLogRepository repository;

        public IntegrationEventLogsController(
            IIntegrationEventLogRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Provides a list of all IntegrationEventLogs
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all IntegrationEventLogs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all IntegrationEventLogs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<IntegrationEventLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<IntegrationEventLog> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides an IntegrationEventLog's details for a particular IntegrationEventLog id
        /// </summary>
        /// <param name="id">IntegrationEventLog id</param>
        /// <response code="200">Ok, if an IntegrationEventLog exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if IntegrationEventLog id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no IntegrationEventLog exists for the given IntegrationEventLog id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>IntegrationEventLog details for the given id</returns>
        [HttpGet("{id}", Name = "GetIntegrationEventLogs")]
        [ProducesResponseType(typeof(IntegrationEventLog), StatusCodes.Status200OK)]
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
        /// Provides a list of all integration event logs by name
        /// </summary>
        /// <response code="200">Ok, a list of all event logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>List of all event logs</returns>
        [HttpGet("IntegrationEventLogsLookup")]
        [ProducesResponseType(typeof(List<IntegrationEventLogLookupViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IntegrationEventLogLookupViewModel AllIntegrationEventLogs()
        {
            var response = repository.Find(null, x => x.IsDeleted == false);
            IntegrationEventLogLookupViewModel eventLogList = new IntegrationEventLogLookupViewModel();

            if (response != null)
            {
                eventLogList.IntegrationEventNameList = new List<string>();
                eventLogList.IntegrationEntityTypeList = new List<string>();

                foreach (var item in response.Items)
                {
                    eventLogList.IntegrationEventNameList.Add(item.IntegrationEventName);
                    eventLogList.IntegrationEntityTypeList.Add(item.EntityType);
                }
                eventLogList.IntegrationEventNameList = eventLogList.IntegrationEventNameList.Distinct().ToList();
                eventLogList.IntegrationEntityTypeList = eventLogList.IntegrationEntityTypeList.Distinct().ToList();
            }
            return eventLogList;
        }

        /// <summary>
        /// Exports the JSONPayload for the specified IntegrationEventLog
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a IntegrationEventLog exists with the given filters</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns> Downloadable JSON file containing the event's payload</returns>
        [HttpGet("ExportPayload/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<object> Export(string id)
        {
            try
            {
                Guid entityID = new Guid(id);
                IntegrationEventLog eventLog = repository.GetOne(entityID);

                if (eventLog == null)
                {
                    ModelState.AddModelError("Export", "Unable to find an IntegrationEventLog for the given ID");
                    return NotFound(ModelState);
                }

                var jsonFile = File(new System.Text.UTF8Encoding().GetBytes(eventLog.PayloadJSON), "text/json", "Payload.JSON");

                return jsonFile;

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Export", ex.Message);
                return ex.GetActionResult();
            }
        }
    }
}
