using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel.Email;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
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
        private readonly IFileManager _fileManager;
        private readonly IEmailManager _manager;

        /// <summary>
        /// EmailAttachmentsController constuctor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userManager"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        /// <param name="manager"></param>
        public EmailAttachmentsController(
            IEmailAttachmentRepository repository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationIdentityUserManager userManager,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IFileManager fileManager,
            IEmailManager manager) : base (repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _fileManager = fileManager;
            _manager = manager;
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
        public async Task<IActionResult> Get(string emailId,
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
                return ex.GetActionResult();
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
        public async Task<IActionResult> GetView(string emailId,
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                ODataHelper<AllEmailAttachmentsViewModel> oDataHelper = new ODataHelper<AllEmailAttachmentsViewModel>();
                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_manager.GetEmailAttachmentsAndNames(Guid.Parse(emailId), oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds email attachments using existing files to the existing email attachments
        /// </summary>
        /// <remarks>
        /// Adds the email attachments with unique email attachment ids to the existing email attachments
        /// </remarks>
        /// <param name="emailId"></param>
        /// <param name="requests"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, new email attachments created and returned</response>
        /// <response code="400">Bad request, when the email attachment values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity</response>
        /// <returns> Newly created unique email attachments</returns>
        [HttpPost("files")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string emailId, [FromBody] string[] requests, string driveName = null)
        {
            try
            {
                var emailAttachments = _manager.AddFileAttachments(emailId, requests, driveName);
                return Ok(emailAttachments);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Attach files to an email
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="files"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, new binary object created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity</response>
        /// <returns> Newly created unique file</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailAttachment), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Attach(string emailId, [FromForm] IFormFile[] files, string driveName = null)
        {
            try
            {
                var emailAttachments = _manager.AddAttachments(files, Guid.Parse(emailId), driveName);
                return Ok(emailAttachments);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
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
        /// <param name="driveName"></param>
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
        public async Task<IActionResult> Put(string id, [FromBody] EmailAttachment request, string driveName = null)
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
                return ex.GetActionResult();
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
                var existingAttachment = _manager.UpdateAttachment(id, request);
                await base.PutEntity(id, existingAttachment);
                return Ok(existingAttachment);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
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
        /// Delete all email attachments with a specified email id from list of email attachments
        /// </summary>
        /// <param name="emailId">Email id to delete all email attachments from - throws bad request if null or empty Guid</param>
        /// <param name="driveName"></param>
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
        public async Task<IActionResult> Delete(string emailId, string driveName = null)
        {
            try
            {
                _manager.DeleteAll(emailId, driveName);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete specific email attachment from list of email attachments
        /// </summary>
        /// <param name="id">Email attachment id to be deleted - throws bad request if null or empty Guid/</param>
        /// <param name="driveName"></param>
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
        public async Task<IActionResult> DeleteAttachment(string id, string driveName = null)
        {
            try
            {
                _manager.DeleteOne(id, driveName);
                await base.DeleteEntity(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Export/download an email attachment file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok if an email attachment file exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email attachment id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email attachment file exists for the given email attachment id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloaded email attachment file</returns>
        [HttpGet("{id}/Export", Name = "ExportEmailAttachment")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ExportEmailAttachment(string id, string driveName = null)
        {
            try
            {
                var fileObject = _manager.Export(id, driveName);
                return File(fileObject.Result?.Content, fileObject.Result?.ContentType, fileObject.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
