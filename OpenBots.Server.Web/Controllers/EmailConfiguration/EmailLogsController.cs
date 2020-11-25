using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;
using OpenBots.Server.Model;
using System.IO;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller for email logs
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailLogsController : EntityController<EmailLog>
    {
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IEmailManager emailSender;
        private readonly IEmailAttachmentRepository emailAttachmentRepository;
        private readonly IEmailAccountRepository emailAccountRepository;

        /// <summary>
        /// EmailLogsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="binaryObjectRepository"></param>
        /// <param name="emailSender"></param>
        /// <param name="emailAttachmentRepository"></param>
        public EmailLogsController(
            IEmailLogRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectRepository binaryObjectRepository,
            IBinaryObjectManager binaryObjectManager,
            IEmailManager emailSender,
            IEmailAttachmentRepository emailAttachmentRepository,
            IEmailAccountRepository emailAccountRepository) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.binaryObjectManager = binaryObjectManager;
            this.binaryObjectRepository = binaryObjectRepository;
            this.emailSender = emailSender;
            this.emailAttachmentRepository = emailAttachmentRepository;
            this.emailAccountRepository = emailAccountRepository;
        }

        /// <summary>
        /// Provides all email logs
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of email logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of email logs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<EmailLogViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<EmailLogViewModel> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany<EmailLogViewModel>();
        }

        /// <summary>
        /// Gets count of email logs in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all email logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all email logs</returns>
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
        /// Get email log by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email log exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email log id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email log exists for the given email log id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email log details</returns>
        [HttpGet("{id}", Name = "GetEmailLogs")]
        [ProducesResponseType(typeof(PaginatedList<EmailLog>), StatusCodes.Status200OK)]
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
        /// Adds a new email log to the existing email logs
        /// </summary>
        /// <remarks>
        /// Adds the email log with unique email log id to the existing email logs
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new email log created and returned</response>
        /// <response code="400">Bad request, when the email log value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique email log</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] EmailLog request)
        {
            try
            {
                await base.PostEntity(request);
                return Ok(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds the email log with unique email log id to existing email logs with status of "Draft"
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new email log created and returned</response>
        /// <response code="400">Bad request, when the email log value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique email log id</returns>
        [HttpPost("compose")]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Compose([FromBody] EmailLog request)
        {
            try
            {
                request.Status = "Draft";
                await base.PostEntity(request);

                return Ok(request.Id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Attach files to an email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="files"></param>
        /// <response code="200">Ok, new binary object created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique binary object</returns>
        [HttpPost("{id}/attach")]
        [ProducesResponseType(typeof(BinaryObject), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Attach(string id, [FromForm] IFormFile[] files)
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
                    string apiComponent = "EmailLogAPI";

                    //Add file to Binary Objects (create entity and put file in EmailLogAPI folder in Server)
                    BinaryObject binaryObject = new BinaryObject()
                    {
                        Name = file.FileName,
                        Folder = apiComponent,
                        CreatedBy = applicationUser?.UserName,
                        CreatedOn = DateTime.UtcNow,
                        CorrelationEntityId = Guid.Parse(id)
                    };

                    string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());
                    //Upload file to Server
                    binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                    binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                    binaryObjectRepository.Add(binaryObject);

                    // Create email attachment
                    EmailAttachment emailAttachment = new EmailAttachment()
                    {
                        Name = binaryObject.Name,
                        BinaryObjectId = binaryObject.Id,
                        ContentType = binaryObject.ContentType,
                        ContentStorageAddress = binaryObject.StoragePath,
                        SizeInBytes = binaryObject.SizeInBytes,
                        EmailLogId = Guid.Parse(id)
                    };
                    emailAttachmentRepository.Add(emailAttachment);
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
        /// Send email draft
        /// </summary>
        /// <param name="id"></param>
        /// <param name="emailMessage"></param>
        /// <response code="200">Ok, if the email log details for the given email log id have been updated</response>
        /// <response code="400">Bad request, if the email log id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>200 Ok response</returns>
        [HttpPut("{id}/send")]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Send(string id, [FromBody] EmailMessage emailMessage, string emailAccountName = null)
        {
            try
            {
                var isEmailAllowed = emailSender.IsEmailAllowed();
                if (!isEmailAllowed)
                {
                    ModelState.AddModelError("Send email", "Email has been disabled.");
                    return BadRequest(ModelState);
                }

                Guid emailLogId = Guid.Parse(id);
                var emailLog = repository.GetOne(emailLogId);

                if (emailLog.Status.Equals("Draft"))
                {
                    var attachments = emailAttachmentRepository.Find(null, q => q.EmailLogId == emailLogId)?.Items;
                    emailMessage.Attachments = attachments;

                    // Email account name is nullable, so it needs ot be used as a query parameter instead of in the put url
                    // If no email account is chosen, the default organization account will be used
                    emailSender.SendEmailAsync(emailMessage, emailAccountName);
                    return Ok();
                }
                else
                {
                    ModelState.AddModelError("Send email", "Email was not able to be sent.");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates email logs
        /// </summary>
        /// <remarks>
        /// Provides an action to update email log, when email log id and the new details of email log are given
        /// </remarks>
        /// <param name="id">Email log id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Email log details to be updated</param>
        /// <response code="200">Ok, if the email log details for the given email log id have been updated</response>
        /// <response code="400">Bad request, if the email log id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] EmailLog request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingEmailLog = repository.GetOne(entityId);
                if (existingEmailLog == null) return NotFound();

                existingEmailLog.EmailAccountId = request.EmailAccountId;
                existingEmailLog.SentOnUTC = request.SentOnUTC;
                existingEmailLog.EmailObjectJson = request.EmailObjectJson;
                existingEmailLog.SenderAddress = request.SenderAddress;
                existingEmailLog.SenderUserId = request.SenderUserId;
                existingEmailLog.Status = request.Status;

                return await base.PutEntity(id, existingEmailLog);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Email Log", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of email log
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of email log is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<EmailLog> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete email log with a specified id from list of email logs
        /// </summary>
        /// <param name="id">Email log id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when email log is soft deleted,(isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email log id is null or empty Guid</response>
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
            var attachments = emailAttachmentRepository.Find(null, q => q.EmailLogId == Guid.Parse(id))?.Items;
            if (attachments.Count != 0)
            {
                foreach (var attachment in attachments)
                {
                    emailAttachmentRepository.SoftDelete((Guid)attachment.Id);
                    binaryObjectRepository.SoftDelete((Guid)attachment.BinaryObjectId);
                }
            }

            return await base.DeleteEntity(id);
        }
    }
}
