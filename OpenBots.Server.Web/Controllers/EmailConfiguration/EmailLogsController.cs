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
    [Route("api/v{version:apiVersion}/[controller]")]
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
        /// Attach a binary object file to an email log
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
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
        public async Task<IActionResult> Attach(string id, [FromForm] IFormFile file)
        {
            try
            {
                string organizationId = binaryObjectManager.GetOrganizationId();
                string apiComponent = "EmailLogAPI";

                //Add file to Binary Objects (create entity and put file in EmailLogAPI folder in Server)
                BinaryObject binaryObject = new BinaryObject();
                binaryObject.Name = file.FileName;
                binaryObject.Folder = apiComponent;
                binaryObject.CreatedOn = DateTime.UtcNow;
                binaryObject.CreatedBy = applicationUser?.UserName;
                binaryObject.CorrelationEntityId = Guid.Parse(id);
                binaryObjectRepository.Add(binaryObject);

                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);

                // Create/ save email attachment entity
                EmailAttachment emailAttachment = new EmailAttachment();
                emailAttachment.Name = binaryObject.Name;
                emailAttachment.BinaryObjectId = binaryObject.Id;
                emailAttachment.ContentType = binaryObject.ContentType;
                emailAttachment.ContentStorageAddress = binaryObject.StoragePath;
                emailAttachment.SizeInBytes = binaryObject.SizeInBytes;
                emailAttachment.EmailLogId = Guid.Parse(id);
                emailAttachmentRepository.Add(emailAttachment);

                //// Attach / add file to email log
                //var emailLog = repository.GetOne(Guid.Parse(id));

                //if (string.IsNullOrEmpty(emailLog.EmailAttachments))
                //{
                //    List<EmailAttachment> attachments = new List<EmailAttachment>();
                //    attachments.Add(emailAttachment);
                //    emailLog.EmailAttachments = JsonConvert.SerializeObject(attachments);
                //}
                //else
                //{
                //    var list = JsonConvert.DeserializeObject<List<EmailAttachment>>(emailLog.EmailAttachments);
                //    list.Add(emailAttachment);
                //    var convertedJson = JsonConvert.SerializeObject(list, Formatting.Indented);
                //    emailLog.EmailAttachments = convertedJson;
                //}
                //repository.Update(emailLog);
                return Ok(emailAttachment);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Send email draft
        /// </summary>
        /// <param name="id"></param>
        /// <param name="emailAccountName"></param>
        /// <param name="emailMessage"></param>
        /// <response code="200">Ok, if the email log details for the given email log id have been updated</response>
        /// <response code="400">Bad request, if the email log id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>200 Ok response</returns>
        [HttpPut("{id}/send/{emailAccountName?}")]
        [ProducesResponseType(typeof(EmailLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Send(string id, string emailAccountName, [FromBody] EmailMessage emailMessage)
        {
            try
            {
                var isEmailAllowed = emailSender.IsEmailAllowed();
                if (!isEmailAllowed)
                {
                    ModelState.AddModelError("Send email", "Email has been disabled.");
                    return BadRequest(ModelState);
                }

                //Use default email account when emailAccountName is not provided
                if (string.IsNullOrEmpty(emailAccountName))
                {
                    var emailAccount = emailAccountRepository.Find(null, q => q.IsDefault).Items?.FirstOrDefault();
                    emailAccountName = emailAccount.Name;
                }

                Guid emailLogId = Guid.Parse(id);
                var emailLog = repository.GetOne(emailLogId);

                if (emailLog.Status.Equals("Draft"))
                {
                    //var attachments = JsonConvert.DeserializeObject<List<EmailAttachment>>(emailLog.EmailAttachments);
                    var attachments = emailAttachmentRepository.Find(null, q => q.EmailLogId == emailLogId)?.Items;
                    emailMessage.Attachments = attachments;

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
            return await base.DeleteEntity(id);
        }
    }
}
