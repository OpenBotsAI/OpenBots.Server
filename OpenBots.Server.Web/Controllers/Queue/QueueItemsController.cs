using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for QueueItems
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class QueueItemsController : EntityController<QueueItem>
    {
        private readonly IQueueItemManager _manager;
        private readonly IHubContext<NotificationHub> _hub;
        public IConfiguration Configuration { get; }
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IOrganizationSettingManager _organizationSettingManager;

        /// <summary>
        /// QueueItemsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="manager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="hub"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="organizationSettingManager"></param>
        /// <param name="webhookPublisher"></param>
        public QueueItemsController(
            IQueueItemRepository repository,
            IQueueItemManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHubContext<NotificationHub> hub,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            IOrganizationSettingManager organizationSettingManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _manager = manager;
            _hub = hub;
            Configuration = configuration;
            _webhookPublisher = webhookPublisher;
            _organizationSettingManager = organizationSettingManager;
        }

        /// <summary>
        /// Provides a list of all queue items
        /// </summary>
        /// <response code="200">Ok, a paginated list of all queue items</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>   
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all queue items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<QueueItem>), StatusCodes.Status200OK)]
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
            [FromQuery(Name = "$skip")] int skip = 0)
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
        /// Provides a view model list of all queue items and corresponding file ids
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all queue items</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all queue items</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllQueueItemsViewModel>), StatusCodes.Status200OK)]
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
                ODataHelper<AllQueueItemsViewModel> oDataHelper = new ODataHelper<AllQueueItemsViewModel>();
                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_manager.GetQueueItemsAndBinaryObjectIds(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));

            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            } 
        }

        /// <summary>
        /// Provides queue item details for a particular queue item id
        /// </summary>
        /// <param name="id">queue item id</param>
        /// <response code="200">Ok, if queue item exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if queue item id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no queue item exists for the given queue item id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Queue item details for the given id</returns>
        [HttpGet("{id}", Name = "GetQueueItem")]
        [ProducesResponseType(typeof(PaginatedList<QueueItem>), StatusCodes.Status200OK)]
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
        /// Provides a queue item's view model details for a particular queue item id
        /// </summary>
        /// <param name="id">Queue item id</param>
        /// <response code="200">Ok, if a queue item exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if queue item id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no queue item exists for the given queue item id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Queue item view model details for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<QueueItemViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    QueueItemViewModel view = okResult.Value as QueueItemViewModel;
                    var queueItem = repository.GetOne(Guid.Parse(id));
                    view = _manager.GetQueueItemView(queueItem);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of queue items in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of all queue items</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of queue items</returns>
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
        /// Deletes a queue item with a specified id from the queue items
        /// </summary>
        /// <param name="id">Queue item id to be deleted - throws bad request if null or empty Guid</param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, when queue item is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if queue item id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id, string driveName = null)
        {
            try
            {
                var existingQueueItem = repository.GetOne(Guid.Parse(id));
                _manager.DeleteQueueItem(existingQueueItem, driveName);

                await _webhookPublisher.PublishAsync("QueueItems.QueueItemDeleted", existingQueueItem.Id.ToString(), existingQueueItem.Name).ConfigureAwait(false);
                var response = await base.DeleteEntity(id);
                _hub.Clients.All.SendAsync("sendnotification", "QueueItem deleted.");

                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of queue item
        /// </summary>
        /// <param name="id">Queue item identifier</param>
        /// <param name="request">Value of the queue item to be updated</param>
        /// <response code="200">Ok, if update of queue item is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial queue item values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<QueueItem> request)
        {
            try
            {
                Guid queueItemId = new Guid(id);
                QueueItem existingQueueItem = repository.GetOne(queueItemId);
                if (existingQueueItem == null) throw new EntityDoesNotExistException("Queue item does not exist or cannot be found");

                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", existingQueueItem.Id.ToString(), existingQueueItem.Name).ConfigureAwait(false);
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Enqueue queue item
        /// </summary>
        /// <param name="request">Value of the queue item to be added</param>
        /// <response code="200">Ok, queue item details</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with queue item details</returns>
        [HttpPost("Enqueue")]
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Enqueue([FromBody] QueueItem request)
        {
            try
            {
                if (_organizationSettingManager.HasDisallowedExecution())
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, "Organization is set to DisallowExecution");

                //create queue item
                var queueItem = _manager.Enqueue(request);
                IActionResult actionResult = await base.PostEntity(queueItem);
                await _webhookPublisher.PublishAsync("QueueItems.NewQueueItemCreated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

                //send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "New queue item added.");

                var queueItemViewModel = _manager.GetQueueItemView(queueItem);

                return Ok(queueItemViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Dequeue queue item
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="queueId"></param>
        /// <response code="200">Ok, queue item</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Dequeued queue item</returns>
        [HttpGet("Dequeue")]
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Dequeue(string agentId, string queueId)
        {
            try
            {
                var response = await _manager.Dequeue(agentId, queueId);
                if (response == null) throw new EntityDoesNotExistException("No item to dequeue from list of queue items");

                //send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "Queue item dequeued.");

                var queueItemViewModel = _manager.GetQueueItemView(response);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItemViewModel.Id.ToString(), queueItemViewModel.Name).ConfigureAwait(false);
                return Ok(queueItemViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Commit queue item
        /// </summary>
        /// <param name="transactionKey">Transaction key id to be verified</param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("Commit")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Commit(string transactionKey, string resultJSON = null)
        {
            try
            {
                var item = _manager.Commit(Guid.Parse(transactionKey), resultJSON).Result;
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", item.Id.ToString(), item.Name).ConfigureAwait(false);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Rollback queue item
        /// </summary>
        /// <param name="transactionKey">Transaction key id to be verified</param>
        /// <param name="errorCode">Error code that has occurred while processing queue item</param>
        /// <param name="errorMessage">Error message that has occurred while processing queue item</param>
        /// <param name="isFatal">Limit to how many retries a queue item can have</param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("Rollback")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Rollback(string transactionKey, string errorCode = null, string errorMessage = null, bool isFatal = false)
        {
            try
            {
                var queueItem = _manager.Rollback(Guid.Parse(transactionKey), errorCode, errorMessage, isFatal).Result;
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(queueItem);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Extend queue item
        /// </summary>
        /// <param name="transactionKey">Transaction key id to be verified</param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("Extend")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Extend(string transactionKey)
        {
            try
            {
                var queueItem = _manager.Extend(Guid.Parse(transactionKey)).Result;
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(queueItem);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates the state and state message of the queue item
        /// </summary>
        /// <param name="transactionKey"></param>
        /// <param name="state"></param>
        /// <param name="stateMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("{id}/state")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateState(string transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null)
        {
            try
            {
                var queueItem = _manager.UpdateState(Guid.Parse(transactionKey), state, stateMessage, errorCode, errorMessage).Result;
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(queueItem);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        ///// <summary>
        ///// Update the queue item with file attachments
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="id"></param>
        ///// <response code="200">Ok response</response>
        ///// <response code="403">Forbidden, unauthorized access</response>
        ///// <response code="422">Unprocessable entity, validation error</response>
        ///// <returns>Ok response</returns>
        //[HttpPut("{id}")]
        //[ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        //[Produces("application/json")]
        //public async Task<IActionResult> UpdateFiles(string id, [FromForm] UpdateQueueItemViewModel request)
        //{
        //    try
        //    {
        //        var queueItem = repository.GetOne(Guid.Parse(id));
        //        var response = _manager.UpdateAttachedFiles(queueItem, request);
        //        await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.GetActionResult();
        //    }
        //}
    }
}