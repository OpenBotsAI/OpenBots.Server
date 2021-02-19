using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.ViewModel.Email;
using OpenBots.Server.Model.Options;
using EmailModel = OpenBots.Server.Model.Configuration.Email;
using OpenBots.Server.Business.Interfaces;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller for emails
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(MyFeatureFlags.Emails)]
    public class EmailsController : EntityController<EmailModel>
    {
        private readonly IEmailManager _manager;
        private readonly IEmailAttachmentRepository _emailAttachmentRepository;
        private readonly IFileManager _fileManager;

        /// <summary>
        /// EmailsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="manager"></param>
        /// <param name="emailAttachmentRepository"></param>
        /// <param name="fileManager"></param>
        public EmailsController(
            IEmailRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IEmailManager manager,
            IEmailAttachmentRepository emailAttachmentRepository,
            IFileManager fileManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _manager = manager;
            _emailAttachmentRepository = emailAttachmentRepository;
            _fileManager = fileManager;
        }

        /// <summary>
        /// Provides all emails
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of emails</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of emails</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<GetEmailsViewModel>), StatusCodes.Status200OK)]
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
                return Ok(base.GetMany<GetEmailsViewModel>());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of emails in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all emails</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all emails</returns>
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
        /// Get email by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email exists for the given email id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email details</returns>
        [HttpGet("{id}", Name = "GetEmailModel")]
        [ProducesResponseType(typeof(PaginatedList<EmailModel>), StatusCodes.Status200OK)]
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
        /// Get email by id view
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email exists for the given email id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email details</returns>
        [HttpGet("{id}/view")]
        [ProducesResponseType(typeof(PaginatedList<EmailViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetView(string id)
        {
            try
            {
                IActionResult actionResult = await base.GetEntity<EmailViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    EmailViewModel view = okResult.Value as EmailViewModel;
                    view = _manager.GetEmailView(view);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new draft email to the existing emails
        /// </summary>
        /// <remarks>
        /// Adds the email with unique email id to the existing emails
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new email created and returned</response>
        /// <response code="400">Bad request, when the email value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique email and attachments, if any</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromForm] AddEmailViewModel request)
        {
            try
            {
                //create email entity
                EmailModel email = _manager.CreateEmail(request);

                //create email attachments & binary objects entities; upload binary object files to server
                var attachments = _manager.AddAttachments(request.Files, email.Id.Value, request.DriveName);
                EmailViewModel emailViewModel = _manager.GetEmailViewModel(email, attachments);

                await base.PostEntity(email);
                return Ok(emailViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Send email draft with file attachments
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="emailAccountName"></param>
        /// <response code="200">Ok, if the email details for the given email id have been updated</response>
        /// <response code="400">Bad request, if the email id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>200 Ok response</returns>
        [HttpPut("{id}/send")]
        [ProducesResponseType(typeof(EmailViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Send(string id, [FromForm] SendEmailViewModel request, string emailAccountName = null)
        {
            try
            {
                var emailViewModel = _manager.SendDraftEmail(id, request, emailAccountName);
                return Ok(emailViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Sends a new email
        /// </summary>
        /// <remarks>
        /// Creates an EmailMessage with file attachments to send to an email address
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="accountName"></param>
        /// <response code="200">Ok, new email message created and sent</response>
        /// <response code="400">Bad request, when the email message value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Ok response</returns>
        [HttpPost("send")]
        [ProducesResponseType(typeof(EmailViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromForm] SendEmailViewModel request, string accountName = null)
        {
            try
            {
                var emailViewModel = _manager.SendNewEmail(request, accountName);
                return Ok(emailViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates email
        /// </summary>
        /// <remarks>
        /// Provides an action to update email, when email id and the new details of email are given
        /// </remarks>
        /// <param name="id">Email id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Email details to be updated</param>
        /// <response code="200">Ok, if the email details for the given email id have been updated</response>
        /// <response code="400">Bad request, if the email id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] EmailModel request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingEmail = repository.GetOne(entityId);
                if (existingEmail == null) return NotFound();

                existingEmail.EmailAccountId = request.EmailAccountId;
                existingEmail.SentOnUTC = request.SentOnUTC;
                existingEmail.EmailObjectJson = request.EmailObjectJson;
                existingEmail.SenderAddress = request.SenderAddress;
                existingEmail.SenderUserId = request.SenderUserId;
                existingEmail.Status = request.Status;
                existingEmail.Direction = request.Direction;
                existingEmail.ConversationId = request.ConversationId;
                existingEmail.ReplyToEmailId = request.ReplyToEmailId;

                return await base.PutEntity(id, existingEmail);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update the email with file attachments
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok response</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPut("{id}/Update")]
        [ProducesResponseType(typeof(UpdateEmailViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateFiles(string id, [FromForm] UpdateEmailViewModel request)
        {
            try
            {
                var response = _manager.UpdateFiles(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of email is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<EmailModel> request)
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
        /// Delete email with a specified id from list of emails
        /// </summary>
        /// <param name="id">Email id to be deleted - throws bad request if null or empty Guid/</param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, when email is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email id is null or empty Guid</response>
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
                _manager.DeleteEmailAttachments(id, driveName);
                return await base.DeleteEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
