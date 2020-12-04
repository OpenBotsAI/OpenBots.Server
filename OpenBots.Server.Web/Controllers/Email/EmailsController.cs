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
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.ViewModel.Email;
using System.Collections.Generic;
using OpenBots.Server.Model;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json;
using AutoMapper;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller for emails
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailsController : EntityController<EmailModel>
    {
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IEmailManager emailSender;
        private readonly IEmailAttachmentRepository emailAttachmentRepository;
        private readonly IBinaryObjectManager binaryObjectManager;

        /// <summary>
        /// EmailsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="binaryObjectRepository"></param>
        /// <param name="emailSender"></param>
        /// <param name="emailAttachmentRepository"></param>
        /// <param name="binaryObjectManager"></param>
        public EmailsController(
            IEmailRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectRepository binaryObjectRepository,
            IEmailManager emailSender,
            IEmailAttachmentRepository emailAttachmentRepository,
            IBinaryObjectManager binaryObjectManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.binaryObjectRepository = binaryObjectRepository;
            this.emailSender = emailSender;
            this.emailAttachmentRepository = emailAttachmentRepository;
            this.binaryObjectManager = binaryObjectManager;
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
        public PaginatedList<GetEmailsViewModel> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany<GetEmailsViewModel>();
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
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
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
        /// Adds a new email to the existing emails
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
                // Create email entity
                EmailModel email = new EmailModel()
                {
                    EmailAccountId = request.EmailAccountId,
                    SenderUserId = request.SenderUserId,
                    CreatedBy = applicationUser?.Name,
                    CreatedOn = DateTime.UtcNow,
                    Status = "New",
                    EmailObjectJson = request.EmailObjectJson,
                    Direction = request.Direction
                };

                await base.PostEntity(email);

                // Create email attachments & binary objects entities; upload files to Server
                var attachments = new List<EmailAttachment>();
                if (request.Files?.Length != 0 && request.Files != null)
                {
                    foreach (var file in request.Files)
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

                        //Add file to Binary Objects (create entity and put file in EmailAPI folder in Server)
                        BinaryObject binaryObject = new BinaryObject()
                        {
                            Name = file.FileName,
                            Folder = apiComponent,
                            CreatedBy = applicationUser?.UserName,
                            CreatedOn = DateTime.UtcNow,
                            CorrelationEntityId = email.Id
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
                            EmailId = (Guid)email.Id,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = applicationUser?.UserName
                        };
                        emailAttachmentRepository.Add(emailAttachment);
                        attachments.Add(emailAttachment);
                    }
                }
                EmailViewModel emailViewModel = new EmailViewModel();
                emailViewModel = emailViewModel.Map(email);
                if (attachments.Count != 0)
                    emailViewModel.Attachments = attachments;
                return Ok(emailViewModel);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds the email with unique email id to existing emails with status of "Draft"
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new email created and returned</response>
        /// <response code="400">Bad request, when the email value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity</response>
        /// <returns> Newly created unique email id</returns>
        [HttpPost("compose")]
        [ProducesResponseType(typeof(EmailModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Compose([FromForm] AddEmailViewModel request)
        {
            try
            {
                // Create email entity
                EmailModel email = new EmailModel()
                {
                    EmailAccountId = request.EmailAccountId,
                    SenderUserId = request.SenderUserId,
                    CreatedBy = applicationUser?.Name,
                    CreatedOn = DateTime.UtcNow,
                    Status = "Draft",
                    EmailObjectJson = request.EmailObjectJson,
                    Direction = request.Direction
                };

                await base.PostEntity(email);

                // Create email attachments & binary objects entities; upload files to Server
                var attachments = new List<EmailAttachment>();
                if (request.Files.Length != 0 && request.Files != null)
                {
                    foreach (var file in request.Files)
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

                        //Add file to Binary Objects (create entity and put file in EmailAPI folder in Server)
                        BinaryObject binaryObject = new BinaryObject()
                        {
                            Name = file.FileName,
                            Folder = apiComponent,
                            CreatedBy = applicationUser?.UserName,
                            CreatedOn = DateTime.UtcNow,
                            CorrelationEntityId = email.Id
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
                            EmailId = (Guid)email.Id,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = applicationUser?.UserName
                        };
                        emailAttachmentRepository.Add(emailAttachment);
                        attachments.Add(emailAttachment);
                    }
                }
                
                EmailViewModel emailViewModel = new EmailViewModel();
                emailViewModel = emailViewModel.Map(email);
                if (attachments.Count != 0)
                    emailViewModel.Attachments = attachments;
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
                EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(request.EmailMessageJson);

                Guid emailId = Guid.Parse(id);
                var email = repository.GetOne(emailId);

                if (email.Status.Equals("Draft"))
                {
                    var attachments = emailAttachmentRepository.Find(null, q => q.EmailId == emailId)?.Items;
                    var emailAttachments = new List<EmailAttachment>();
                    if (request.Files != null)
                    {
                        var files = request.Files.ToList();
                        // Replace attachments with new ones
                        foreach (var attachment in attachments)
                        {
                            var binaryObject = binaryObjectRepository.GetOne((Guid)attachment.BinaryObjectId);
                            bool exists = false;
                            // Check if file with same hash and queue item id already exists
                            foreach (var file in request.Files)
                            {
                                byte[] bytes = Array.Empty<byte>();
                                using (var ms = new MemoryStream())
                                {
                                    await file.CopyToAsync(ms);
                                    bytes = ms.ToArray();
                                }

                                string hash = string.Empty;
                                using (SHA256 sha256Hash = SHA256.Create())
                                {
                                    hash = binaryObjectManager.GetHash(sha256Hash, bytes);
                                }

                                if (binaryObject.HashCode == hash && binaryObject.CorrelationEntityId == email.Id)
                                {
                                    exists = true;
                                    files.Remove(file);
                                }
                            }
                            // If queue item attachment already exists: continue
                            if (exists)
                                continue;
                            // If queue item attachment doesn't exist: remove attachment and binary object
                            else
                            {
                                binaryObjectRepository.SoftDelete((Guid)attachment.BinaryObjectId);
                                emailAttachmentRepository.SoftDelete((Guid)attachment.Id);
                            }
                        }
                        // If file doesn't exist in binary objects: add binary object entity, upload file, and add queue item attachment entity
                        foreach (var file in files)
                        {
                            var binaryObj = binaryObjectRepository.Find(null, q => q.Name == file.Name && q.ContentType == file.ContentType && q.SizeInBytes == file.Length)?.Items?.FirstOrDefault();
                            if (binaryObj == null)
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

                                //Create binary object
                                BinaryObject binaryObject = new BinaryObject()
                                {
                                    Name = file.FileName,
                                    Folder = apiComponent,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = applicationUser?.UserName,
                                    CorrelationEntityId = Guid.Parse(id)
                                };

                                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                                //Upload file to the Server
                                binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                                binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                                binaryObjectRepository.Add(binaryObject);

                                //Create queue item attachment
                                EmailAttachment emailAttachment = new EmailAttachment()
                                {
                                    BinaryObjectId = (Guid)binaryObject.Id,
                                    EmailId = Guid.Parse(id),
                                    CreatedBy = applicationUser?.UserName,
                                    CreatedOn = DateTime.UtcNow,
                                    SizeInBytes = binaryObject.SizeInBytes,
                                    ContentStorageAddress = binaryObject.StoragePath,
                                    ContentType = binaryObject.ContentType,
                                    Name = binaryObject.Name
                                };
                                emailAttachmentRepository.Add(emailAttachment);
                                emailAttachments.Add(emailAttachment);
                            }
                        }
                        emailMessage.Attachments = emailAttachments;
                    }
                    else
                        emailMessage.Attachments = attachments;

                    // Email account name is nullable, so it needs ot be used as a query parameter instead of in the put url
                    // If no email account is chosen, the default organization account will be used
                    await emailSender.SendEmailAsync(emailMessage, emailAccountName, id, "Outgoing");

                    email = repository.Find(null, q => q.Id == emailId)?.Items?.FirstOrDefault();
                    EmailViewModel emailViewModel = new EmailViewModel();
                    emailViewModel = emailViewModel.Map(email);
                    if (attachments.Count != 0 && attachments != null)
                        emailViewModel.Attachments = attachments;
                    else emailViewModel.Attachments = emailAttachments;
                    return Ok(emailViewModel);
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
                EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(request.EmailMessageJson);

                // Create Email Attachment entities for each file attached
                var attachments = new List<EmailAttachment>();

                Guid id = Guid.NewGuid();

                if (request.Files?.Length != 0 && request.Files != null)
                {
                    foreach (var file in request.Files)
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

                        //Add file to Binary Objects (create entity and put file in EmailAPI folder in Server)
                        BinaryObject binaryObject = new BinaryObject()
                        {
                            Name = file.FileName,
                            Folder = apiComponent,
                            CreatedBy = applicationUser?.UserName,
                            CreatedOn = DateTime.UtcNow,
                            CorrelationEntityId = id
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
                            EmailId = id,
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = applicationUser?.UserName
                        };
                        emailAttachmentRepository.Add(emailAttachment);
                        attachments.Add(emailAttachment);

                        // Add attachment entities to email message
                        emailMessage.Attachments = attachments;
                    }
                    await emailSender.SendEmailAsync(emailMessage, accountName, id.ToString(), "Outgoing");
                }
                EmailModel email = repository.Find(null, q => q.Id == id)?.Items?.FirstOrDefault();
                EmailViewModel emailViewModel = new EmailViewModel();
                emailViewModel = emailViewModel.Map(email);
                if (attachments.Count != 0)
                    emailViewModel.Attachments = attachments;
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
                ModelState.AddModelError("Email", ex.Message);
                return BadRequest(ModelState);
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
            var email = repository.GetOne(Guid.Parse(id));
            if (email == null) return NotFound();

            email.ConversationId = request.ConversationId;
            email.Direction = request.Direction;
            email.EmailObjectJson = request.EmailObjectJson;
            email.SenderAddress = request.SenderAddress;
            email.SenderName = request.SenderName;
            email.SenderUserId = applicationUser?.PersonId;
            email.Status = request.Status;
            email.EmailAccountId = request.EmailAccountId;
            email.ReplyToEmailId = request.ReplyToEmailId;
            email.Reason = request.Reason;
            email.SentOnUTC = request.SentOnUTC;

            var attachments = emailAttachmentRepository.Find(null, q => q.EmailId == Guid.Parse(id))?.Items;
            var binaryObjectIds = new List<Guid>();
            var files = request.Files.ToList();

            foreach (var attachment in attachments)
            {
                var binaryObject = binaryObjectRepository.GetOne((Guid)attachment.BinaryObjectId);
                bool exists = false;
                // Check if file with same hash and queue item id already exists
                foreach (var file in request.Files)
                {
                    byte[] bytes = Array.Empty<byte>();
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        bytes = ms.ToArray();
                    }

                    string hash = string.Empty;
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        hash = binaryObjectManager.GetHash(sha256Hash, bytes);
                    }

                    if (binaryObject.HashCode == hash && binaryObject.CorrelationEntityId == email.Id)
                    {
                        exists = true;
                        files.Remove(file);
                        binaryObjectIds.Add((Guid)binaryObject.Id);
                    }
                }
                // If queue item attachment already exists: continue
                if (exists)
                    continue;
                // If queue item attachment doesn't exist: remove attachment and binary object
                else
                {
                    binaryObjectRepository.SoftDelete((Guid)attachment.BinaryObjectId);
                    emailAttachmentRepository.SoftDelete((Guid)attachment.Id);
                }
            }
            // If file doesn't exist in binary objects: add binary object entity, upload file, and add queue item attachment entity
            var emailAttachments = new List<EmailAttachment>();
            foreach (var file in files)
            {
                var binaryObj = binaryObjectRepository.Find(null, q => q.Name == file.Name && q.ContentType == file.ContentType && q.SizeInBytes == file.Length)?.Items?.FirstOrDefault();
                if (binaryObj == null)
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

                    //Create binary object
                    BinaryObject binaryObject = new BinaryObject()
                    {
                        Name = file.FileName,
                        Folder = apiComponent,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = applicationUser?.UserName,
                        CorrelationEntityId = Guid.Parse(id)
                    };

                    string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                    //Upload file to the Server
                    binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                    binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                    binaryObjectRepository.Add(binaryObject);

                    //Create queue item attachment
                    EmailAttachment emailAttachment = new EmailAttachment()
                    {
                        BinaryObjectId = (Guid)binaryObject.Id,
                        EmailId = Guid.Parse(id),
                        CreatedBy = applicationUser?.UserName,
                        CreatedOn = DateTime.UtcNow,
                        SizeInBytes = binaryObject.SizeInBytes,
                        ContentStorageAddress = binaryObject.StoragePath,
                        ContentType = binaryObject.ContentType,
                        Name = binaryObject.Name,
                    };
                    emailAttachmentRepository.Add(emailAttachment);
                    binaryObjectIds.Add((Guid)emailAttachment.BinaryObjectId);
                    emailAttachments.Add(emailAttachment);
                }
            }
            // Update queue item
            repository.Update(email);

            EmailViewModel response = new EmailViewModel();
            response = response.Map(email);
            if (attachments.Count != 0 && attachments != null)
                response.Attachments = attachments;
            else
                response.Attachments = emailAttachments;
            return Ok(response);
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
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete email with a specified id from list of emails
        /// </summary>
        /// <param name="id">Email id to be deleted - throws bad request if null or empty Guid/</param>
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
        public async Task<IActionResult> Delete(string id)
        {
            var attachments = emailAttachmentRepository.Find(null, q => q.EmailId == Guid.Parse(id))?.Items;
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
