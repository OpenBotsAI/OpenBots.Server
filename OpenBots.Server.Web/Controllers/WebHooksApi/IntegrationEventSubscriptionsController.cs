using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.WebHooksApi
{
    /// <summary>
    /// Controller for Integration Event Subscriptions
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationEventSubscriptionsController : EntityController<IntegrationEventSubscription>
    {
        public IntegrationEventSubscriptionsController(
            IIntegrationEventSubscriptionRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
        }

        /// <summary>
        /// Provides a list of all Integration Event Subscriptions
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all Integration Event Subscriptions</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all IntegrationEventSubscription</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<IntegrationEventSubscription>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<IntegrationEventSubscription> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides an IntegrationEventSubscription's details for a particular IntegrationEventSubscription id
        /// </summary>
        /// <param name="id">IntegrationEventSubscription id</param>
        /// <response code="200">Ok, if an IntegrationEventSubscription exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if IntegrationEventSubscription id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no IntegrationEventSubscription exists for the given IntegrationEventSubscription id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>IntegrationEventSubscription details for the given id</returns>
        [HttpGet("{id}", Name = "GetIntegrationEventSubscription")]
        [ProducesResponseType(typeof(IntegrationEventSubscription), StatusCodes.Status200OK)]
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
        /// Adds a new IntegrationEventSubscription to the existing IntegrationEventSubscriptions
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new IntegrationEventSubscription created and returned</response>
        /// <response code="400">Bad request, when the IntegrationEventSubscription value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique IntegrationEventSubscription</returns>
        [HttpPost]
        [ProducesResponseType(typeof(IntegrationEventSubscription), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] IntegrationEventSubscription request)
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
        /// Updates an IntegrationEventSubscription 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an IntegrationEventSubscription, when id and the new details of 
        /// IntegrationEventSubscription are given
        /// </remarks>
        /// <param name="id">IntegrationEventSubscription Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">IntegrationEventSubscription details to be updated</param>
        /// <response code="200">Ok, if the IntegrationEventSubscription details for the given 
        /// IntegrationEventSubscription id have been updated</response>
        /// <response code="400">Bad request, if the IntegrationEventSubscription id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] IntegrationEventSubscription request)
        {
            try
            {
                Guid entityId = new Guid(id);
                IntegrationEventSubscription existingEventSubscription = repository.GetOne(entityId);

                if (existingEventSubscription == null)
                {
                    ModelState.AddModelError("Export", "Unable to find an IntegrationEventSubscription for the given ID");
                    return NotFound(ModelState);
                }

                existingEventSubscription.Name = request.Name;
                existingEventSubscription.EntityType = request.EntityType;
                existingEventSubscription.IntegrationEventName = request.IntegrationEventName;
                existingEventSubscription.EntityID = request.EntityID;
                existingEventSubscription.EntityName = request.EntityName;
                existingEventSubscription.TransportType = request.TransportType;
                existingEventSubscription.HTTP_URL = request.HTTP_URL;
                existingEventSubscription.HTTP_AddHeader_Key = request.HTTP_AddHeader_Key;
                existingEventSubscription.HTTP_AddHeader_Value = request.HTTP_AddHeader_Value;
                existingEventSubscription.HTTP_Max_RetryCount = request.HTTP_Max_RetryCount;
                existingEventSubscription.QUEUE_QueueID = request.QUEUE_QueueID;

                return await base.PutEntity(id, existingEventSubscription);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Update", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes an IntegrationEventSubscription with a specified id
        /// </summary>
        /// <param name="id">IntegrationEventSubscription id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when IntegrationEventSubscription is soft deleted, 
        /// (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if IntegrationEventSubscription id is null or empty Guid</response>
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
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of IntegrationEventSubscription
        /// </summary>
        /// <param name="id">IntegrationEventSubscription identifier</param>
        /// <param name="request">Value of the IntegrationEventSubscription to be updated</param>
        /// <response code="200">Ok, if update of IntegrationEventSubscription is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial IntegrationEventSubscription values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<IntegrationEventSubscription> request)
        {          
            return await base.PatchEntity(id, request);
        }
    }
 }