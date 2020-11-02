using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for QueueItems
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class QueueItemsController : EntityController<QueueItem>
    {
        readonly IQueueItemManager manager;
        readonly IQueueRepository queueRepository;
        private IHubContext<NotificationHub> _hub;
        public IConfiguration Configuration { get; }
        
        /// <summary>
        /// QueueItemsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="manager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="hub"></param>
        /// <param name="httpContextAccessor"></param>
        public QueueItemsController(
            IQueueItemRepository repository,
            IQueueRepository queueRepository,
            IQueueItemManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHubContext<NotificationHub> hub,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            _hub = hub;
            this.queueRepository = queueRepository;
            Configuration = configuration;
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
        [ProducesResponseType(typeof(PaginatedList<QueueItemViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<QueueItemViewModel> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany<QueueItemViewModel>();
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
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Deletes a queue item with a specified id from the queue items
        /// </summary>
        /// <param name="id">queue item id to be deleted - throws bad request if null or empty Guid</param>
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
        public async Task<IActionResult> Delete(string id)
        {
            Guid queueItemId = new Guid(id);
            QueueItem queueItem = repository.GetOne(queueItemId);

            if (queueItem.IsLocked)
            {
                ModelState.AddModelError("Delete", "Queue Item is locked at this time and cannot be deleted");
                return BadRequest(ModelState);
            }

            var response = await base.DeleteEntity(id);
            _hub.Clients.All.SendAsync("sendnotification", "QueueItem deleted.");

            return response;
        }

        /// <summary>
        /// Updates partial details of queue item
        /// </summary>
        /// <param name="id">queue item identifier</param>
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
            return await base.PatchEntity(id, request);
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
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Enqueue([FromBody] QueueItem request)
        {
            try
            {
                var response = await manager.Enqueue(request);

                await base.PostEntity(response);
                //Send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "New queue item added.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Enqueue", ex.Message);
                return BadRequest(ModelState);
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
        [ProducesResponseType(typeof(QueueItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Dequeue(string agentId, string queueId)
        {
            try
            {
                var response = await manager.Dequeue(agentId, queueId);

                if (response == null)
                {
                    ModelState.AddModelError("Dequeue", "No item to dequeue from list of queue items.");
                    return BadRequest(ModelState);
                }

                //Send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "Queue item dequeued.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Dequeue", ex.Message);
                return BadRequest(ModelState);
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
        public async Task<IActionResult> Commit(string transactionKey)
        {
            try
            {
                Guid transactionKeyId = Guid.Parse(transactionKey);
                QueueItem queueItem = await manager.GetQueueItem(transactionKeyId);

                if (queueItem == null)
                {
                    ModelState.AddModelError("Rollback", "Transaction key cannot be found.");
                    return BadRequest(ModelState);
                }

                Guid queueItemId = (Guid)queueItem.Id;

                await manager.Commit(queueItemId, transactionKeyId);

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Commit", ex.Message);
                return BadRequest(ModelState);
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
                Guid transactionKeyId = Guid.Parse(transactionKey);
                QueueItem queueItem = await manager.GetQueueItem(transactionKeyId);

                if (queueItem == null)
                {
                    ModelState.AddModelError("Rollback", "Transaction key cannot be found.");
                    return BadRequest(ModelState);
                }

                Guid queueItemId = (Guid)queueItem.Id;
                Guid queueId = queueItem.QueueId;
                Queue queue = queueRepository.GetOne(queueId);
                int retryLimit = queue.MaxRetryCount;

                if (retryLimit == null || retryLimit == 0)
                {
                    retryLimit = int.Parse(Configuration["Queue.Global:DefaultMaxRetryCount"]);
                }

                await manager.Rollback(queueItemId, transactionKeyId, retryLimit, errorCode, errorMessage, isFatal);

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Rollback", ex.Message);
                return BadRequest(ModelState);
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
                Guid transactionKeyId = Guid.Parse(transactionKey);
                QueueItem queueItem = await manager.GetQueueItem(transactionKeyId);

                if (queueItem == null)
                {
                    ModelState.AddModelError("Rollback", "Transaction key cannot be found.");
                    return BadRequest(ModelState);
                }

                Guid queueItemId = (Guid)queueItem.Id;

                var response = manager.Extend(queueItemId, transactionKeyId);

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Extend", ex.Message);
                return BadRequest(ModelState);
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
                Guid transactionKeyId = Guid.Parse(transactionKey);
                QueueItem queueItem = await manager.GetQueueItem(transactionKeyId);

                if (queueItem == null)
                {
                    ModelState.AddModelError("Update State", "Transaction key cannot be found.");
                }

                Guid queueItemId = (Guid)queueItem.Id;

                var response = manager.UpdateState(queueItemId, transactionKeyId, state, stateMessage, errorCode, errorMessage);

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Update State", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
