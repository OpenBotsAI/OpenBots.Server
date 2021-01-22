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
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IQueueItemRepository queueItemRepository;
        private readonly IQueueItemManager manager;
        private readonly IWebhookPublisher webhookPublisher;

        /// <summary>
        /// QueueItemAttachmentsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userManager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="binaryObjectRepository"></param>
        /// <param name="queueItemRepository"></param>
        /// <param name="manager"></param>
        /// <param name="webhookPublisher"></param>
        public QueueItemAttachmentsController(
            IQueueItemAttachmentRepository repository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationIdentityUserManager userManager,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IQueueItemRepository queueItemRepository,
            IQueueItemManager manager,
            IWebhookPublisher webhookPublisher) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.binaryObjectRepository = binaryObjectRepository;
            this.binaryObjectManager = binaryObjectManager;
            this.queueItemRepository = queueItemRepository;
            this.manager = manager;
            this.webhookPublisher = webhookPublisher;
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
        public PaginatedList<AllQueueItemAttachmentsViewModel> GetView(string queueItemId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            ODataHelper<AllQueueItemAttachmentsViewModel> oDataHelper = new ODataHelper<AllQueueItemAttachmentsViewModel>();

            var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

            return manager.GetQueueItemAttachmentsAndNames(Guid.Parse(queueItemId), oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);
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
        /// Adds queue item attachments using existing binary objects to the existing queue item attachments
        /// </summary>
        /// <remarks>
        /// Adds the queue item attachments with unique queue item attachment ids to the existing queue item attachments
        /// </remarks>
        /// <param name="queueItemId"></param>
        /// <param name="requests"></param>
        /// <response code="200">Ok, new queue item attachments created and returned</response>
        /// <response code="400">Bad request, when the queue item attachment values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique queue item attachments</returns>
        [HttpPost("binaryObjects")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string queueItemId, [FromBody] string[] requests)
        {
            try
            {
                var entityId = new Guid(queueItemId);
                var queueItem = queueItemRepository.Find(0, 1).Items?.Where(q => q.Id.ToString() == queueItemId).FirstOrDefault();
                long? payload = 0;

                if (requests.Length == 0 || requests == null)
                {
                    ModelState.AddModelError("Attach", "No files uploaded to attach");
                    return BadRequest(ModelState);
                }

                foreach (var request in requests)
                {
                    var binaryObject = binaryObjectRepository.Find(null, q => q.Id == Guid.Parse(request))?.Items?.FirstOrDefault();
                    if (binaryObject == null)
                    {
                        ModelState.AddModelError("Save", "No file attached");
                        return BadRequest(ModelState);
                    }

                    long? size = binaryObject.SizeInBytes;
                    if (size <= 0)
                    {
                        ModelState.AddModelError("File attachment", $"File size of file {binaryObject.Name} cannot be 0");
                        return BadRequest(ModelState);
                    }

                    //create queue item attachment
                    QueueItemAttachment queueItemAttachment = new QueueItemAttachment()
                    {
                        BinaryObjectId = (Guid)binaryObject.Id,
                        QueueItemId = Guid.Parse(queueItemId),
                        CreatedBy = applicationUser?.UserName,
                        CreatedOn = DateTime.UtcNow,
                        SizeInBytes = (long)binaryObject.SizeInBytes
                    };
                    repository.Add(queueItemAttachment);
                    payload += queueItemAttachment.SizeInBytes;
                }

                //update queue item payload
                queueItem.PayloadSizeInBytes += (long)payload;
                queueItemRepository.Update(queueItem);
                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

                var queueItemAttachments = repository.Find(null).Items?.Where(q => q.QueueItemId == entityId);
                return Ok(queueItemAttachments);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Attach", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Attach files to a queue item
        /// </summary>
        /// <param name="queueItemId"></param>
        /// <param name="files"></param>
        /// <response code="200">Ok, new binary object created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique binary object</returns>
        [HttpPost]
        [ProducesResponseType(typeof(QueueItemAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Attach(string queueItemId, [FromForm] IFormFile[] files)
        {
            try
            {
                Guid entityId = new Guid(queueItemId);
                var queueItem = queueItemRepository.Find(0, 1).Items?.Where(q => q.Id == entityId).FirstOrDefault();

                var binaryObjects = manager.AttachFiles(files.ToList(), entityId, queueItem);
                foreach (var binaryObject in binaryObjects)
                    await webhookPublisher.PublishAsync("Files.NewFileCreated", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);
                await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

                var queueItemAttachments = repository.Find(null).Items?.Where(q => q.QueueItemId == entityId);
                return Ok(queueItemAttachments);
            }
            catch (FileNotFoundException ex)
            {
                ModelState.AddModelError("Save file", ex.Message);
                return NotFound(ModelState);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("File attachment", ex.Message);
                return UnprocessableEntity(ModelState);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Attach", ex.Message);
                return BadRequest(ModelState);
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
        public async Task<IActionResult> Put(string queueItemId, string id, [FromForm] IFormFile file)
        {
            try
            {
                Guid entityId = new Guid(id);
                Guid queueItemEntityId = new Guid(queueItemId);
                var queueItem = queueItemRepository.GetOne(queueItemEntityId);
                var existingAttachment = repository.GetOne(entityId);
                if (existingAttachment == null) return NotFound();

                queueItem.PayloadSizeInBytes -= existingAttachment.SizeInBytes;

                string binaryObjectId = existingAttachment.BinaryObjectId.ToString();
                var binaryObject = binaryObjectRepository.GetOne(Guid.Parse(binaryObjectId));

                string organizationId = binaryObject.OrganizationId.ToString();
                if (!string.IsNullOrEmpty(organizationId))
                    organizationId = binaryObjectManager.GetOrganizationId().ToString();

                if (file == null)
                {
                    ModelState.AddModelError("Save", "No attachment uploaded");
                    return BadRequest(ModelState);
                }

                long size = file == null ? 0 : file.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("File Upload", $"File size of attachment {file.FileName} cannot be 0");
                    return BadRequest(ModelState);
                }

                try
                {
                    existingAttachment.SizeInBytes = file.Length;

                    if (existingAttachment.BinaryObjectId != Guid.Empty && size > 0)
                    {
                        //update attachment file in OpenBots.Server.Web using relative directory
                        string apiComponent = "QueueItemAPI";
                        binaryObjectManager.Update(file, organizationId, apiComponent, Guid.Parse(binaryObjectId));
                        await webhookPublisher.PublishAsync("Files.FileUpdated", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);
                    }

                    //update queue item payload
                    queueItem.PayloadSizeInBytes += file.Length;
                    queueItemRepository.Update(queueItem);
                    await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

                    //update attachment entity
                    await base.PutEntity(id, existingAttachment);
                    return Ok(existingAttachment);
                }
                catch (Exception ex)
                {
                    return ex.GetActionResult();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Queue Item Attachment", ex.Message);
                return BadRequest(ModelState);
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
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete all queue item attachments with a specified queue item id from list of queue item attachments
        /// </summary>
        /// <param name="queueItemId">Queue item id to delete all queue item attachments from - throws bad request if null or empty Guid/</param>
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
        public async Task<IActionResult> Delete(string queueItemId)
        {
            Guid entityId = new Guid(queueItemId);

            var attachments = repository.Find(null, q => q.QueueItemId == entityId)?.Items;
            if (attachments.Count != 0)
            {
                foreach (var attachment in attachments)
                {
                    repository.SoftDelete((Guid)attachment.Id);
                    var otherAttachment = repository.Find(0, 1).Items?.Where(q => q.IsDeleted == false && q.BinaryObjectId == attachment.BinaryObjectId).FirstOrDefault();
                    if (otherAttachment == null)
                    {
                        var binaryObject = binaryObjectRepository.GetOne(attachment.BinaryObjectId);
                        binaryObjectRepository.SoftDelete(attachment.BinaryObjectId);
                        await webhookPublisher.PublishAsync("Files.FileDeleted", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);
                    }
                }
            }

            //update queue item payload
            var queueItem = queueItemRepository.Find(0, 1).Items?.Where(q => q.Id == entityId).FirstOrDefault();
            queueItem.PayloadSizeInBytes = 0;
            queueItemRepository.Update(queueItem);
            await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

            return Ok();
        }

        /// <summary>
        /// Delete specific queue item attachment from list of queue item attachments
        /// </summary>
        /// <param name="id">Queue item attachment id to be deleted - throws bad request if null or empty Guid/</param>
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
        public async Task<IActionResult> DeleteAttachment(string id)
        {
            Guid entityId = new Guid(id);

            var attachment = repository.Find(null, q => q.Id == entityId)?.Items?.FirstOrDefault();
            if (attachment != null)
            {
                await base.DeleteEntity(id);
                var otherAttachment = repository.Find(0, 1).Items?.Where(q => q.IsDeleted == false && q.BinaryObjectId == attachment.BinaryObjectId).FirstOrDefault();
                if (otherAttachment == null)
                {
                    var binaryObject = binaryObjectRepository.GetOne(attachment.BinaryObjectId);
                    binaryObjectRepository.SoftDelete(attachment.BinaryObjectId);
                    await webhookPublisher.PublishAsync("Files.FileDeleted", binaryObject.Id.ToString(), binaryObject.Name).ConfigureAwait(false);
                }
            }
            else
            {
                ModelState.AddModelError("Delete Attachment", "Attachment could not be found");
                return BadRequest(ModelState);
            }

            //update queue item payload
            var queueItem = queueItemRepository.Find(0, 1).Items?.Where(q => q.Id == attachment.QueueItemId).FirstOrDefault();
            queueItem.PayloadSizeInBytes -= attachment.SizeInBytes;
            queueItemRepository.Update(queueItem);
            await webhookPublisher.PublishAsync("QueueItems.QueueItemUpdated", queueItem.Id.ToString(), queueItem.Name).ConfigureAwait(false);

            return Ok();
        }
    }
}