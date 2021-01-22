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
using OpenBots.Server.ViewModel.Email;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.Email
{
    /// <summary>
    /// Controller for email attachments
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/Emails/{emailId}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailAttachmentsController : EntityController<EmailAttachment>
    {
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IEmailManager manager;

        /// <summary>
        /// EmailAttachmentsController constuctor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userManager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        /// <param name="binaryObjectRepository"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="manager"></param>
        public EmailAttachmentsController(
            IEmailAttachmentRepository repository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationIdentityUserManager userManager,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IBinaryObjectRepository binaryObjectRepository,
            IBinaryObjectManager binaryObjectManager,
            IEmailManager manager) : base (repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.binaryObjectRepository = binaryObjectRepository;
            this.binaryObjectManager = binaryObjectManager;
            this.manager = manager;
        }

        /// <summary>
        /// Provides all email attachments for an email
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of email attachments</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of email attachments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Get(string emailId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                var attachments = repository.Find(null, q => q.EmailId == Guid.Parse(emailId));
                return Ok(attachments);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Email Attachments", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Gets count of email attachments related to an email in the database
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all email attachments</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all email attachments</returns>
        [HttpGet("count")]
        [Produces(typeof(IActionResult))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCount(string emailId,
        [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                var count = repository.Find(null, q => q.EmailId == Guid.Parse(emailId))?.Items.Count;
                return Ok(count);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }

        }

        /// <summary>
        /// Get email attachment by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email attachment exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email attachment id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email attachment exists for the given email attachment id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email attachment details</returns>
        [HttpGet("{id}", Name = "GetEmailAttachments")]
        [ProducesResponseType(typeof(PaginatedList<EmailAttachment>), StatusCodes.Status200OK)]
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
        /// Provides all email attachments view for an email
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of email attachments view</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of email attachments view</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllEmailAttachmentsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AllEmailAttachmentsViewModel> GetView(string emailId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            ODataHelper<AllEmailAttachmentsViewModel> oDataHelper = new ODataHelper<AllEmailAttachmentsViewModel>();

            var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

            return manager.GetEmailAttachmentsAndNames(Guid.Parse(emailId), oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);
        }

        /// <summary>
        /// Adds email attachments using existing binary objects to the existing email attachments
        /// </summary>
        /// <remarks>
        /// Adds the email attachments with unique email attachment ids to the existing email attachments
        /// </remarks>
        /// <param name="emailId"></param>
        /// <param name="requests"></param>
        /// <response code="200">Ok, new email attachments created and returned</response>
        /// <response code="400">Bad request, when the email attachment values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique email attachments</returns>
        [HttpPost("binaryObjects")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string emailId, [FromBody] string[] requests)
        {
            try
            {
                if (requests.Length == 0 || requests == null)
                {
                    ModelState.AddModelError("Attach", "No files uploaded to attach");
                    return BadRequest(ModelState);
                }

                var emailAttachments = new List<EmailAttachment>();

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

                    //create email attachment
                    EmailAttachment emailAttachment = new EmailAttachment()
                    {
                        Name = binaryObject.Name,
                        BinaryObjectId = binaryObject.Id,
                        ContentType = binaryObject.ContentType,
                        ContentStorageAddress = binaryObject.StoragePath,
                        SizeInBytes = binaryObject.SizeInBytes,
                        EmailId = Guid.Parse(emailId),
                        CreatedBy = applicationUser?.UserName,
                        CreatedOn = DateTime.UtcNow
                    };
                    repository.Add(emailAttachment);
                    emailAttachments.Add(emailAttachment);
                }
                return Ok(emailAttachments);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Attach", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Attach files to an email
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="files"></param>
        /// <response code="200">Ok, new binary object created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique binary object</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Attach(string emailId, [FromForm] IFormFile[] files)
        {
            try
            {
                if (files.Length == 0 || files == null)
                {
                    ModelState.AddModelError("Attach", "No files uploaded to attach");
                    return BadRequest(ModelState);
                }

                var emailAttachments = new List<EmailAttachment>();

                foreach (var file in files)
                {
                    if (file == null)
                    {
                        ModelState.AddModelError("Save", "No file attached");
                        return BadRequest(ModelState);
                    }

                    long size = file.Length;
                    if (size <= 0)
                    {
                        ModelState.AddModelError("File attachment", $"File size of file {file.FileName} cannot be 0");
                        return BadRequest(ModelState);
                    }

                    string organizationId = binaryObjectManager.GetOrganizationId();
                    string apiComponent = "EmailAPI";

                    //add file to binary objects (create entity and put file in EmailAPI folder in Server)
                    BinaryObject binaryObject = new BinaryObject()
                    {
                        Name = file.FileName,
                        Folder = apiComponent,
                        CreatedBy = applicationUser?.UserName,
                        CreatedOn = DateTime.UtcNow,
                        CorrelationEntityId = Guid.Parse(emailId)
                    };

                    string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());
                    //upload file to Server
                    binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                    binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                    binaryObjectRepository.Add(binaryObject);

                    //create email attachment
                    EmailAttachment emailAttachment = new EmailAttachment()
                    {
                        Name = binaryObject.Name,
                        BinaryObjectId = binaryObject.Id,
                        ContentType = binaryObject.ContentType,
                        ContentStorageAddress = binaryObject.StoragePath,
                        SizeInBytes = binaryObject.SizeInBytes,
                        EmailId = Guid.Parse(emailId),
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = applicationUser?.UserName
                    };
                    repository.Add(emailAttachment);
                    emailAttachments.Add(emailAttachment);
                }
                return Ok(emailAttachments);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Attach", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates email attachment
        /// </summary>
        /// <remarks>
        /// Provides an action to update email attachment, when email attachment id and the new details of email attachment are given
        /// </remarks>
        /// <param name="id">Email attachment id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Email attachment details to be updated</param>
        /// <response code="200">Ok, if the email attachment details for the given email attachment id have been updated</response>
        /// <response code="400">Bad request, if the email attachment id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] EmailAttachment request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingAttachment = repository.GetOne(entityId);
                if (existingAttachment == null) return NotFound();

                existingAttachment.Name = request.Name;

                return await base.PutEntity(id, existingAttachment);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Email Attachment", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates an email attachment with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an email attachment with file, when email attachment id and the new details of the email attachment are given
        /// </remarks>
        /// <param name="id">Email attachment id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">New file to update email attachment</param>
        /// <response code="200">Ok, if the email attachment details for the given email attachment id have been updated</response>
        /// <response code="400">Bad request, if the email attachment id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated email attachment value</returns>
        [HttpPut("{id}/Update")]
        [ProducesResponseType(typeof(EmailAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromForm] UpdateEmailAttachmentViewModel request)
        {
            try
            {
                Guid entityId = new Guid(id);
                var existingAttachment = repository.GetOne(entityId);
                if (existingAttachment == null) return NotFound();

                string binaryObjectId = existingAttachment.BinaryObjectId.ToString();
                var binaryObject = binaryObjectRepository.GetOne(Guid.Parse(binaryObjectId));

                string organizationId = binaryObject.OrganizationId.ToString();
                if (!string.IsNullOrEmpty(organizationId))
                    organizationId = binaryObjectManager.GetOrganizationId().ToString();

                if (request.file == null)
                {
                    ModelState.AddModelError("Save", "No attachment uploaded");
                    return BadRequest(ModelState);
                }

                long size = request.file == null ? 0 : request.file.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("File Upload", $"File size of attachment {request.file.FileName} cannot be 0");
                    return BadRequest(ModelState);
                }

                try
                {
                    if (!string.IsNullOrEmpty(request.file.FileName))
                        existingAttachment.Name = request.file.FileName;

                    existingAttachment.ContentType = request.file.ContentType;
                    existingAttachment.SizeInBytes = request.file.Length;

                    if (existingAttachment.BinaryObjectId != Guid.Empty && size > 0)
                    {
                        //update attachment file in OpenBots.Server.Web using relative directory
                        string apiComponent = "EmailAPI";
                        binaryObjectManager.Update(request.file, organizationId, apiComponent, Guid.Parse(binaryObjectId));
                    }

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
                ModelState.AddModelError("Email Attachment", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of email attachment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of email attachment is successful</response>
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
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<EmailAttachment> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete all email attachments with a specified email id from list of email attachments
        /// </summary>
        /// <param name="emailId">Email id to delete all email attachments from - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when email attachments are soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string emailId)
        {
            var attachments = repository.Find(null, q => q.EmailId == Guid.Parse(emailId))?.Items;
            if (attachments.Count != 0)
            {
                foreach (var attachment in attachments)
                {
                    repository.SoftDelete((Guid)attachment.Id);
                    binaryObjectRepository.SoftDelete((Guid)attachment.BinaryObjectId);
                }
            }
            return Ok();
        }

        /// <summary>
        /// Delete specific email attachment from list of email attachments
        /// </summary>
        /// <param name="id">Email attachment id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when email attachment is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email attachment id is null or empty Guid</response>
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
            var attachment = repository.Find(null, q => q.Id == Guid.Parse(id))?.Items?.FirstOrDefault();
            if (attachment != null)
            {
                await base.DeleteEntity(id);
                binaryObjectRepository.SoftDelete((Guid)attachment.BinaryObjectId);
            }
            else
            {
                ModelState.AddModelError("Delete Attachment", "Attachment could not be found");
                return BadRequest(ModelState);
            }
            return Ok();
        }
    }
}
