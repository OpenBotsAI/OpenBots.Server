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
    /// Controller for Integration events
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationEventsController : ReadOnlyEntityController<IntegrationEvent>
    {
        private readonly IIntegrationEventRepository repository;

        /// <summary>
        /// IntegrationEventsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public IntegrationEventsController(
            IIntegrationEventRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Provides a list of all IntegrationEvents
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all IntegrationEvents</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all IntegrationEvents</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<IntegrationEvent>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<IntegrationEvent> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides an IntegrationEvent's details for a particular IntegrationEvent id
        /// </summary>
        /// <param name="id">IntegrationEvent id</param>
        /// <response code="200">Ok, if an IntegrationEvent exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if IntegrationEvent id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no IntegrationEvent exists for the given IntegrationEvent id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>IntegrationEvent details for the given id</returns>
        [HttpGet("{id}", Name = "GetIntegrationEvent")]
        [ProducesResponseType(typeof(IntegrationEvent), StatusCodes.Status200OK)]
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
        /// Provides a list of all IntegrationEvent Entity names
        /// </summary>
        /// <response code="200">Ok, a list of all event Entity names</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>List of all names in IntegrationEvents table</returns>
        [HttpGet("IntegrationEventLookup")]
        [ProducesResponseType(typeof(List<IntegrationEventEntitiesLookupViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IntegrationEventEntitiesLookupViewModel AllIntegrationEvents()
        {
            var response = repository.Find(null, x => x.IsDeleted == false);
            IntegrationEventEntitiesLookupViewModel eventLogList = new IntegrationEventEntitiesLookupViewModel();

            if (response != null)
            {
                eventLogList.EntityNameList = new List<string>();

                foreach (var item in response.Items)
                {
                    eventLogList.EntityNameList.Add(item.EntityType);
                }
                eventLogList.EntityNameList = eventLogList.EntityNameList.Distinct().ToList();            }
            return eventLogList;
        }
    }
}
