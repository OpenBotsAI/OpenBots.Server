using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.QueueItem;
using OpenBots.Server.Web.Hubs;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QueueModel = OpenBots.Server.Model.Queue;

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
        readonly IQueueItemManager manager;
        readonly IQueueRepository queueRepository;
        private IHubContext<NotificationHub> _hub;
        private IHubManager hubManager;
        readonly IScheduleRepository scheduleRepo;
        public IConfiguration Configuration { get; }
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IQueueItemAttachmentRepository queueItemAttachmentRepository;
        private readonly IWebhookPublisher webhookPublisher;
        private readonly IOrganizationSettingManager organizationSettingManager;

        /// <summary>
        /// QueueItemsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="manager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="hub"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="binaryObjectRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="hubManager"></param>
        /// <param name="queueItemAttachmentRepository"></param>
        /// <param name="queueRepository"></param>
        /// <param name="scheduleRepository"></param>
        /// <param name="webhookPublisher"></param>
        public QueueItemsController(
            IQueueItemRepository repository,
            IQueueRepository queueRepository,
            IQueueItemManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHubContext<NotificationHub> hub,
            IHttpContextAccessor httpContextAccessor,
            IHubManager hubManager,
            IScheduleRepository scheduleRepository,
            IConfiguration configuration,
            IBinaryObjectRepository binaryObjectRepository,
            IQueueItemAttachmentRepository queueItemAttachmentRepository,
            IWebhookPublisher webhookPublisher,
            IOrganizationSettingManager organizationSettingManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            _hub = hub;
            this.queueRepository = queueRepository;
            this.hubManager = hubManager;
            scheduleRepo = scheduleRepository;
            Configuration = configuration;
            this.binaryObjectRepository = binaryObjectRepository;
            this.queueItemAttachmentRepository = queueItemAttachmentRepository;
            this.webhookPublisher = webhookPublisher;
            this.organizationSettingManager = organizationSettingManager;
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
        public PaginatedList<QueueItem> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a view model list of all queue items and corresponding binary object ids
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
        public PaginatedList<AllQueueItemsViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            ODataHelper<AllQueueItemsViewModel> oDataHelper = new ODataHelper<AllQueueItemsViewModel>();

            var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

            return manager.GetQueueItemsAndBinaryObjectIds(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);
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
                    view = manager.GetQueueItemView(view, id);
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
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Deletes a queue item with a specified id from the queue items
        /// </summary>
        /// <param name="id">Queue item id to be deleted - throws bad request if null or empty Guid</param>
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
            QueueItem existingQueueItem = repository.GetOne(queueItemId);
            if (existingQueueItem == null)
            {
                ModelState.AddModelError("QueueItem", "QueueItem cannot be found or does not exist.");
                return NotFound(ModelState);
            }
            if (existingQueueItem.IsLocked)
            {
                ModelState.AddModelError("Delete", "Queue Item is locked at this time and cannot be deleted");
                return BadRequest(ModelState);
            }

            await webhookPublisher.PublishAsync("QueueItems.QueueItemDeleted", existingQueueItem.Id.ToString(), existingQueueItem.Name).ConfigureAwait(false);
            var response = await base.DeleteEntity(id);
            _hub.Clients.All.SendAsync("sendnotification", "QueueItem deleted.");

            //soft delete each queue item attachment entity and binary object entity that correlates to the queue item
            var attachmentsList = queueItemAttachmentRepository.Find(null, q => q.QueueItemId == existingQueueItem.Id)?.Items;
            foreach (var attachment in attachmentsList)
            {
                queueItemAttachmentRepository.SoftDelete((Guid)attachment.Id);

                var existingBinary = binaryObjectRepository.GetOne(attachment.BinaryObjectId);
                if (existingBinary != null)
                {
                    await webhookPublisher.PublishAsync("Files.FileDeleted", existingBinary.Id.ToString(), existingBinary.Name).ConfigureAwait(false);
                }
                binaryObjectRepository.SoftDelete(attachment.BinaryObjectId);
            }

            return response;
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
            Guid queueItemId = new Guid(id);
            QueueItem existingQueueItem = repository.GetOne(queueItemId);
            if (existingQueueItem == null)
            {
                ModelState.AddModelError("QueueItem", "QueueItem cannot be found or does not exist.");
            }

            await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", existingQueueItem.Id.ToString(), existingQueueItem.Name).ConfigureAwait(false);
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
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Enqueue([FromBody] QueueItem request)
        {
            try
            {
                if (organizationSettingManager.HasDisallowedExecution())
                {
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, "Organization is set to DisallowExecution");
                }

                var response = await manager.Enqueue(request);

                //check if a queue arrival schedule exists for this queue
                Schedule existingSchedule = scheduleRepo.Find(0, 1).Items?.Where(s => s.QueueId == response.QueueId)?.FirstOrDefault();
                if (existingSchedule != null && existingSchedule.IsDisabled == false && existingSchedule.StartingType.ToLower().Equals("queuearrival"))
                {
                    Schedule schedule = new Schedule();
                    schedule.AgentId = existingSchedule.AgentId;
                    schedule.CRONExpression = "";
                    schedule.LastExecution = DateTime.UtcNow;
                    schedule.NextExecution = DateTime.UtcNow;
                    schedule.IsDisabled = false;
                    schedule.ProjectId = null;
                    schedule.StartingType = "QueueArrival";
                    schedule.Status = "New";
                    schedule.ExpiryDate = DateTime.UtcNow.AddDays(1);
                    schedule.StartDate = DateTime.UtcNow;
                    schedule.AutomationId = existingSchedule.AutomationId;

                    var jsonScheduleObj = System.Text.Json.JsonSerializer.Serialize(schedule);
                    //call GetScheduleParameters()
                    var jobId = BackgroundJob.Enqueue(() => hubManager.ExecuteJob(jsonScheduleObj, Enumerable.Empty<ParametersViewModel>()));
                }

                QueueItem queueItem = new QueueItem()
                {
                    CreatedBy = applicationUser.Name,
                    CreatedOn = DateTime.UtcNow,
                    DataJson = response.DataJson,
                    Event = response.Event,
                    ExpireOnUTC = response.ExpireOnUTC,
                    IsLocked = response.IsLocked,
                    JsonType = response.JsonType,
                    Name = response.Name,
                    PostponeUntilUTC = response.PostponeUntilUTC,
                    Priority = response.Priority,
                    QueueId = response.QueueId,
                    RetryCount = response.RetryCount,
                    Source = response.Source,
                    State = response.State,
                    StateMessage = response.StateMessage,
                    Type = response.Type,
                    PayloadSizeInBytes = 0
                };

                //create queue item
                IActionResult actionResult = await base.PostEntity(queueItem);
                await webhookPublisher.PublishAsync("QueueItems.NewQueueItemCreated", response.Id.ToString(), response.Name).ConfigureAwait(false);

                //send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "New queue item added.");

                QueueItemViewModel queueItemViewModel = new QueueItemViewModel();
                queueItemViewModel = queueItemViewModel.Map(queueItem);
                string id = response.Id.ToString();
                queueItemViewModel = manager.GetQueueItemView(queueItemViewModel, id);

                return Ok(queueItemViewModel);
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
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
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

                //send SignalR notification to all connected clients
                await _hub.Clients.All.SendAsync("sendnotification", "Queue item dequeued.");

                QueueItemViewModel queueItemViewModel = new QueueItemViewModel();
                queueItemViewModel = queueItemViewModel.Map(response);
                string id = response.Id.ToString();
                queueItemViewModel = manager.GetQueueItemView(queueItemViewModel, id);

                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItemViewModel.Id.ToString(), queueItemViewModel.Name).ConfigureAwait(false);
                return Ok(queueItemViewModel);
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
        public async Task<IActionResult> Commit(string transactionKey, string resultJSON = null)
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

                var item = await manager.Commit(queueItemId, transactionKeyId, resultJSON);
                if (item.State == "New")
                {
                    ModelState.AddModelError("Commit", "Queue item lock time has expired.  Adding back to queue and trying again.");
                    return BadRequest(ModelState);
                }
                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(item);
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
                QueueModel queue = queueRepository.GetOne(queueId);
                int retryLimit = queue.MaxRetryCount;

                if (retryLimit == null || retryLimit == 0)
                {
                    retryLimit = int.Parse(Configuration["Queue.Global:DefaultMaxRetryCount"]);
                }
                var item = await manager.Rollback(queueItemId, transactionKeyId, retryLimit, errorCode, errorMessage, isFatal);
                if (item.State == "Failed")
                {
                    ModelState.AddModelError("Rollback", item.StateMessage);
                    return BadRequest(ModelState);
                }
                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(item);
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
                var item = await manager.Extend(queueItemId, transactionKeyId);

                if (item.State == "New")
                {
                    ModelState.AddModelError("Extend", "Queue item was not able to be extended.  Locked until time has passed.  Adding back to queue and trying again.");
                    return BadRequest(ModelState);
                }

                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(item);
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
                    return BadRequest(ModelState);
                }

                Guid queueItemId = (Guid)queueItem.Id;

                var item = await manager.UpdateState(queueItemId, transactionKeyId, state, stateMessage, errorCode, errorMessage);
                if (item.State == "New")
                {
                    ModelState.AddModelError("Extend", "Queue item state was not able to be updated.  Locked until time has passed.  Adding back to queue and trying again.");
                    return BadRequest(ModelState);
                }

                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok(item);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Update State", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update the queue item with file attachments
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QueueItemViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateFiles(string id, [FromForm] UpdateQueueItemViewModel request)
        {
            var queueItem = repository.GetOne(Guid.Parse(id));
            if (queueItem == null) return NotFound();

            queueItem.DataJson = request.DataJson;
            queueItem.Event = request.Event;
            queueItem.ExpireOnUTC = request.ExpireOnUTC;
            queueItem.PostponeUntilUTC = request.PostponeUntilUTC;
            queueItem.Name = request.Name;
            queueItem.QueueId = (Guid)request.QueueId;
            queueItem.Source = request.Source;
            queueItem.Type = request.Type;
            queueItem.State = request.State;

            if (queueItem.State == "New")
            {
                queueItem.StateMessage = null;
                queueItem.RetryCount = 0;
            }
            await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

            //attach new files
            var binaryObjects = manager.UpdateAttachedFiles(request, queueItem);
            foreach (var binaryObject in binaryObjects)
                await webhookPublisher.PublishAsync("Files.NewFileCreated", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);

            QueueItemViewModel response = new QueueItemViewModel();
            response = response.Map(queueItem);
            var binaryObjectIds = new List<Guid?>();
            foreach (var binaryObject in binaryObjects)
                binaryObjectIds.Add(binaryObject.Id);
            response.BinaryObjectIds = binaryObjectIds;
            return Ok(response);
        }
    }
}