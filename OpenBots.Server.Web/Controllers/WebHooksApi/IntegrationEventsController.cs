using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.Lookup;
using OpenBots.Server.Web.Webhooks;
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
    public class IntegrationEventsController : EntityController<IntegrationEvent>
    {
        private readonly IIntegrationEventRepository _repository;
        private readonly IIntegrationEventManager _integrationEventManager;


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
            IIntegrationEventManager integrationEventManager,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _repository = repository;
            _integrationEventManager = integrationEventManager;
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
        public async Task<IActionResult> AllIntegrationEvents()
        {
            try
            {
                var response = _repository.Find(null, x => x.IsDeleted == false);
                IntegrationEventEntitiesLookupViewModel eventLogList = new IntegrationEventEntitiesLookupViewModel();

                if (response != null)
                {
                    eventLogList.EntityNameList = new List<string>();

                    foreach (var item in response.Items)
                    {
                        eventLogList.EntityNameList.Add(item.EntityType);
                    }
                    eventLogList.EntityNameList = eventLogList.EntityNameList.Distinct().ToList();
                }
                return Ok(eventLogList);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new business event
        /// </summary>
        /// <remarks>
        /// Adds a business event with a unique id to the existing IntegrationEvents
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new event created and returned</response>
        /// <response code="400">Bad request, when the event value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created business event details</returns>
        [HttpPost("BusinessEvent")]
        [ProducesResponseType(typeof(IntegrationEvent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PostBusinessEvent([FromBody] CreateBusinessEventViewModel request)
        {
            try
            {

                IntegrationEvent businessEvent = request.Map(request);
                return await base.PostEntity(businessEvent);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Raise a new business event
        /// </summary>
        /// <param name="request">Request to raise new business event</param>
        /// <param name="id">IntegrationEvent id, produces bad request if id is null or ids don't match</param>
        /// <response code="200">Ok, new event raised and returned</response>
        /// <response code="400">Bad request, when the event value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Ok if business event was successfully raised</returns>
        [HttpPost("BusinessEvent/RaiseEvent/{id}")]
        [ProducesResponseType(typeof(IntegrationEvent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RaiseBusinessEvent(string id, [FromBody] RaiseBusinessEventViewModel request)
        {
            try
            {
                _integrationEventManager.RaiseBusinessEvent(id, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update a business event
        /// </summary>
        /// <remarks>
        /// Provides an action to update a business event, when IntegrationEvent id and the new details of the business event are given
        /// </remarks>
        /// <param name="id">IntegrationEvent id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Business event details to be updated</param>
        /// <response code="200">Ok, if the IntegrationEvent details for the given id have been updated</response>
        /// <response code="400">Bad request, if the IntegrationEvent id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated business event details for the IntegrationEvent</returns>
        [HttpPut("BusinessEvent/{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> PutBusinessEvent(string id, [FromBody] CreateBusinessEventViewModel request)
        {
            try
            {
                IntegrationEvent businessEvent = _integrationEventManager.UpdateBusinessEvent(id, request);
                return await base.PutEntity(id, businessEvent);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of a business event
        /// </summary>
        /// <param name="id">IntegrationEvent identifier</param>
        /// <param name="request">Value of the business event to be updated</param>
        /// <response code="200">Ok, if update of business event is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity ,validation error</response>
        /// <returns>Ok response, if the partial business event values have been updated</returns>
        [HttpPatch("BusinessEvent/{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> PatchBusinessEvent(string id,
            [FromBody] JsonPatchDocument<IntegrationEvent> request)
        {
            try
            {
                Guid entityId = new Guid(id);
                var existingEvent = _repository.GetOne(entityId);

                if (existingEvent == null) throw new EntityDoesNotExistException($"IntegrationEvent with id {id} could not be found");

                if (existingEvent.IsSystem == true) throw new UnauthorizedOperationException($"System events can't be updated", EntityOperationType.Update);

                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
