using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.Queue
{
    /// <summary>
    /// Controller for queue item attachments
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/QueueItems/{queueItemId}/[controller]")]
    [ApiController]
    [Authorize]
    public class QueueItemAttachmentsController : EntityController<QueueItemAttachment>
    {
        private readonly IQueueItemRepository _queueItemRepository;
        private readonly IQueueItemManager _manager;
        private readonly IWebhookPublisher _webhookPublisher;

        /// <summary>
        /// QueueItemAttachmentsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userManager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        /// <param name="queueItemRepository"></param>
        /// <param name="manager"></param>
        /// <param name="webhookPublisher"></param>
        public QueueItemAttachmentsController(
            IQueueItemAttachmentRepository repository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationIdentityUserManager userManager,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IQueueItemRepository queueItemRepository,
            IQueueItemManager manager,
            IWebhookPublisher webhookPublisher) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _queueItemRepository = queueItemRepository;
            _manager = manager;
            _webhookPublisher = webhookPublisher;
        }

        /// <summary>
        /// Provides all queue item attachments for a queue item
        /// </summary>
        /// <param name="queueItemId"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of queue item attachments</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of queue item attachments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Get(string queueItemId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                var attachments = repository.Find(null, q => q.QueueItemId == Guid.Parse(queueItemId));
                return Ok(attachments);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Queue Item Attachments", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Provides all queue item attachments view for a queue item
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of queue item attachments view</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of queue item attachments view</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllQueueItemAttachmentsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetView(string queueItemId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                ODataHelper<AllQueueItemAttachmentsViewModel> oDataHelper = new ODataHelper<AllQueueItemAttachmentsViewModel>();
                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_manager.GetQueueItemAttachmentsAndNames(Guid.Parse(queueItemId), oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of queue item attachments related to a queue item in the database
        /// </summary>
        /// <param name="queueItemId"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all queue item attachments</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all queue item attachments</returns>
        [HttpGet("count")]
        [Produces(typeof(IActionResult))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCount(string queueItemId,
        [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                var count = repository.Find(null, q => q.QueueItemId == Guid.Parse(queueItemId))?.Items.Count;
                return Ok(count);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Get queue item attachment by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a queue item attachment exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if queue item attachment id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no queue item attachment exists for the given queue item attachment id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Queue item attachment details</returns>
        [HttpGet("{id}", Name = "GetQueueItemAttachments")]
        [ProducesResponseType(typeof(PaginatedList<QueueItemAttachment>), StatusCodes.Status200OK)]
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
        /// Adds queue item attachments using existing files to the existing queue item attachments
        /// </summary>
        /// <remarks>
        /// Adds the queue item attachments with unique queue item attachment ids to the existing queue item attachments
        /// </remarks>
        /// <param name="queueItemId"></param>
        /// <param name="requests"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, new queue item attachments created and returned</response>
        /// <response code="400">Bad request, when the queue item attachment values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity</response>
        /// <returns> Newly created unique queue item attachments</returns>
        [HttpPost("files")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string queueItemId, [FromBody] string[] requests, string driveName = null)
        {
            try
            {
                var queueItem = _queueItemRepository.GetOne(Guid.Parse(queueItemId));
                var queueItemAttachments = _manager.AddFileAttachments(queueItem, requests, driveName);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItemId, queueItem.Name).ConfigureAwait(false);
                return Ok(queueItemAttachments);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Attach files to a queue item
        /// </summary>
        /// <param name="queueItemId"></param>
        /// <param name="files"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, new binary object created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity</response>
        /// <returns> Newly created unique binary object</returns>
        [HttpPost]
        [ProducesResponseType(typeof(QueueItemAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Attach(string queueItemId, [FromForm] IFormFile[] files, string driveName = null)
        {
            try
            {
                var queueItem = _queueItemRepository.GetOne(Guid.Parse(queueItemId));
                var queueItemAttachments = _manager.AddNewAttachments(queueItem, files, driveName);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItemId, queueItem.Name).ConfigureAwait(false);

                return Ok(queueItemAttachments);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a queue item attachment with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a queue item attachment with file, when queue item attachment id and the new details of the queue item attachment are given
        /// </remarks>
        /// <param name="queueItemId">Queue item id</param>
        /// <param name="id">Queue item attachment id, produces bad request if id is null or ids don't match</param>
        /// <param name="file">New file to update queue item attachment</param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, if the queue item attachment details for the given queue item attachment id have been updated</response>
        /// <response code="400">Bad request, if the queue item attachment id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated queue item attachment value</returns>
        [HttpPut("{id}/Update")]
        [ProducesResponseType(typeof(QueueItemAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string queueItemId, string id, [FromForm] IFormFile file, string driveName = null)
        {
            try
            {
                var queueItem = _queueItemRepository.GetOne(Guid.Parse(queueItemId));
                var existingAttachment = _manager.UpdateAttachment(queueItem, id, file, driveName);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                await base.PutEntity(id, existingAttachment);
                return Ok(existingAttachment);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of queue item attachment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of queue item attachment is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailAttachment), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<QueueItemAttachment> request)
        {
            try
            {
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete all queue item attachments with a specified queue item id from list of queue item attachments
        /// </summary>
        /// <param name="queueItemId">Queue item id to delete all queue item attachments from - throws bad request if null or empty Guid/</param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, when queue item attachments are soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if queue item id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string queueItemId, string driveName = null)
        {
            try
            {
                var queueItem = _queueItemRepository.GetOne(Guid.Parse(queueItemId));
                _manager.DeleteAll(queueItem, driveName);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete specific queue item attachment from list of queue item attachments
        /// </summary>
        /// <param name="id">Queue item attachment id to be deleted - throws bad request if null or empty Guid/</param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, when queue item attachment is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if queue item attachment id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DeleteAttachment(string id, string driveName = null)
        {
            try
            {
                var attachment = repository.GetOne(Guid.Parse(id));
                var queueItem = _queueItemRepository.Find(null).Items?.Where(q => q.Id == attachment.QueueItemId).FirstOrDefault();
                _manager.DeleteOne(attachment, queueItem, driveName);
                await base.DeleteEntity(id);
                await _webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}