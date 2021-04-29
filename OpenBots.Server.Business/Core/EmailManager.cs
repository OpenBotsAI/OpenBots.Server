using MimeKit;
using Newtonsoft.Json;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Infrastructure.Azure.Email;
using OpenBots.Server.Infrastructure.Email;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.ViewModel.Email;
using System.IO;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using System.Security.Cryptography;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.ViewModel.File;
using IOFile = System.IO.File;
using System.Text;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.Business
{
    public class EmailManager : BaseManager, IEmailManager
    {
        private readonly IPersonRepository _personRepo;
        private readonly IPersonEmailRepository _personEmailRepository;
        private readonly IEmailAccountRepository _emailAccountRepository;
        private readonly IEmailRepository _emailRepository;
        private readonly IEmailSettingsRepository _emailSettingsRepository;
        private readonly IOrganizationManager _organizationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailAttachmentRepository _emailAttachmentRepository;
        private readonly IFileManager _fileManager;
        private readonly IStorageDriveRepository _storageDriveRepository;
        private readonly IStorageFileRepository _storageFileRepository;
        private ApplicationUser _applicationUser { get; set; }

        public EmailManager(
            IPersonRepository personRepo,
            IPersonEmailRepository personEmailRepository,
            IEmailAccountRepository emailAccountRepository,
            IEmailRepository emailRepository,
            IEmailSettingsRepository emailSettingsRepository,
            IOrganizationManager organizationManager,
            IHttpContextAccessor httpContextAccessor,
            IEmailAttachmentRepository emailAttachmentRepository,
            IFileManager fileManager,
            IStorageDriveRepository storageDriveRepository,
            IStorageFileRepository storageFileRepository)
        {
            _personRepo = personRepo;
            _personEmailRepository = personEmailRepository;
            _emailAccountRepository = emailAccountRepository;
            _emailRepository = emailRepository;
            _emailSettingsRepository = emailSettingsRepository;
            _organizationManager = organizationManager;
            _httpContextAccessor = httpContextAccessor;
            _emailAttachmentRepository = emailAttachmentRepository;
            _fileManager = fileManager;
            _storageDriveRepository = storageDriveRepository;
            _storageFileRepository = storageFileRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _personRepo.SetContext(userSecurityContext);
            _personEmailRepository.SetContext(userSecurityContext);
            _emailAccountRepository.SetContext(userSecurityContext);
            _emailRepository.SetContext(userSecurityContext);
            _emailSettingsRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Email CreateEmail(AddEmailViewModel request)
        {
            Email email = new Email()
            {
                EmailAccountId = request.EmailAccountId,
                SenderUserId = request.SenderUserId,
                CreatedBy = _applicationUser?.Name,
                CreatedOn = DateTime.UtcNow,
                Status = StatusType.Draft.ToString(),
                EmailObjectJson = request.EmailObjectJson,
                Direction = request.Direction
            };
            return email;
        }

        public EmailViewModel GetEmailViewModel(Email email, List<EmailAttachment> attachments)
        {
            EmailViewModel emailViewModel = new EmailViewModel();
            emailViewModel = emailViewModel.Map(email);
            if (attachments.Count != 0 && attachments != null)
                emailViewModel.Attachments = attachments;
            return emailViewModel;
        }

        public List<EmailAttachment> AddAttachments(IFormFile[] files, Guid id, string driveId)
        {
            driveId = CheckDriveId(driveId);
            var drive = GetDrive(driveId);
            driveId = drive.Id.ToString();

            var attachments = new List<EmailAttachment>();
            if (files?.Length != 0 && files != null)
            {
                //add files to drive
                string storagePath = Path.Combine(drive.Name, "Email Attachments", id.ToString());
                var fileView = new FileFolderViewModel()
                {
                    StoragePath = storagePath,
                    FullStoragePath = storagePath,
                    Files = files,
                    IsFile = true
                };

                long? size = 0;
                foreach (var file in files)
                    size += file.Length;

                CheckStoragePathExists(fileView, size, id, driveId, drive.Name);
                var fileViewList = _fileManager.AddFileFolder(fileView, driveId);

                foreach (var file in fileViewList)
                {
                    //create email attachment
                    EmailAttachment emailAttachment = new EmailAttachment()
                    {
                        Name = file.Name,
                        FileId = file.Id,
                        ContentType = file.ContentType,
                        ContentStorageAddress = file.FullStoragePath,
                        SizeInBytes = file.Size,
                        EmailId = id,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name
                    };
                    _emailAttachmentRepository.Add(emailAttachment);
                    attachments.Add(emailAttachment);
                }
            }
            else throw new EntityOperationException("No files found to attach");

            return attachments;
        }

        public Task SendEmailAsync(EmailMessage emailMessage, string accountName = null, string id = null, string direction = null)
        {
            Email emailObject = new Email();
            Guid? emailId = Guid.NewGuid();

            if (!string.IsNullOrEmpty(id))
            {
                emailId = Guid.Parse(id);
                emailObject = _emailRepository.Find(null, q => q.Id == emailId)?.Items?.FirstOrDefault();
                if (emailObject == null)
                {
                    emailObject = new Email()
                    {
                        Id = emailId,
                        Status = StatusType.Unknown.ToString()
                    };
                }
            }

            Email email = new Email();
            if (!string.IsNullOrEmpty(id))
                email.Id = emailId;

            //find email settings and determine is email is enabled/disabled
            var organizationId = Guid.Parse(_organizationManager.GetDefaultOrganization().Id.ToString());
            var emailSettings = _emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();
            //check if accountName exists
            var existingAccount = _emailAccountRepository.Find(null, d => d.Name.ToLower(null) == accountName?.ToLower(null))?.Items?.FirstOrDefault();
            if (existingAccount == null)
                existingAccount = _emailAccountRepository.Find(null, d => d.IsDefault && !d.IsDisabled).Items.FirstOrDefault();

            //if there are no records in the email settings table for that organization, email should be disabled
            if (emailSettings == null)
            {
                email.Status = StatusType.Blocked.ToString();
                email.Reason = "Email disabled.  Please configure email settings.";
            }
            //if there are email settings but they are disabled, don't send email
            else if (emailSettings != null && emailSettings.IsEmailDisabled)
            {
                email.Status = StatusType.Blocked.ToString();
                email.Reason = "Email functionality has been disabled.";
            }
            else
            {
                if (existingAccount == null && emailSettings != null)
                {
                    existingAccount = _emailAccountRepository.Find(null, a => a.IsDefault == true && a.IsDisabled == false)?.Items?.FirstOrDefault();
                    if (existingAccount == null)
                    {
                        email.Status = StatusType.Failed.ToString();
                        email.Reason = $"Account '{accountName}' could be found.";
                    }
                    if (existingAccount != null && existingAccount.IsDisabled == true)
                    {
                        email.Status = StatusType.Blocked.ToString();
                        email.Reason = $"Account '{accountName}' has been disabled.";
                    }
                }
                //set from email address
                else if (existingAccount != null)
                {
                    EmailAddress fromEmailAddress = new EmailAddress(existingAccount.FromName, existingAccount.FromEmailAddress);
                    List<EmailAddress> emailAddresses = new List<EmailAddress>();

                    foreach (EmailAddress emailAddress in emailMessage.From)
                    {
                        if (!string.IsNullOrEmpty(emailAddress.Address))
                            emailAddresses.Add(emailAddress);
                    }
                    emailMessage.From.Clear();
                    foreach (EmailAddress emailAddress in emailAddresses)
                        emailMessage.From.Add(emailAddress);
                    emailMessage.From.Add(fromEmailAddress);
                }

                //remove email addresses in to, cc, and bcc lists with domains that are blocked or not allowed
                List<EmailAddress> toList = new List<EmailAddress>();
                List<EmailAddress> ccList = new List<EmailAddress>();
                List<EmailAddress> bccList = new List<EmailAddress>();

                if (string.IsNullOrEmpty(emailSettings.AllowedDomains))
                {
                    if (!string.IsNullOrEmpty(emailSettings.BlockedDomains))
                    {
                        //remove any email address that is in blocked domain
                        IEnumerable<string>? denyList = (new List<string>(emailSettings?.BlockedDomains?.Split(','))).Select(s => s.ToLowerInvariant().Trim());
                        foreach (EmailAddress address in emailMessage.To)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    toList.Add(address);
                            }
                        }
                        emailMessage.To.Clear();
                        emailMessage.To = toList;

                        foreach (EmailAddress address in emailMessage.CC)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    ccList.Add(address);
                            }
                        }
                        emailMessage.CC.Clear();
                        emailMessage.CC = ccList;

                        foreach (EmailAddress address in emailMessage.Bcc)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    bccList.Add(address);
                            }
                        }
                        emailMessage.Bcc.Clear();
                        emailMessage.Bcc = bccList;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(emailSettings.AllowedDomains))
                    {
                        //remove any email address that is not on white list
                        IEnumerable<string> allowList = (new List<string>(emailSettings.AllowedDomains.Split(','))).Select(s => s.ToLowerInvariant().Trim());
                        foreach (EmailAddress address in emailMessage.To)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (allowList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    toList.Add(address);
                            }
                        }
                        emailMessage.To.Clear();
                        emailMessage.To = toList;

                        foreach (EmailAddress address in emailMessage.CC)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (allowList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    ccList.Add(address);
                            }
                        }
                        emailMessage.CC.Clear();
                        emailMessage.CC = ccList;

                        foreach (EmailAddress address in emailMessage.Bcc)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailboxAddress mailAddress = new MailboxAddress(address.Address);
                                if (allowList.Contains(mailAddress.Address.Split("@")[1].ToLowerInvariant()))
                                    bccList.Add(address);
                            }
                        }
                        emailMessage.Bcc.Clear();
                        emailMessage.Bcc = bccList;
                    }
                }

                if (emailMessage.To.Count == 0)
                {
                    email.Status = StatusType.Blocked.ToString();
                    email.Reason = "No email addresses to send email to.";
                }

                //add any necessary additional email addresses (administrators, etc.)
                if (!string.IsNullOrEmpty(emailSettings.AddToAddress))
                {
                    foreach (string toAddress in emailSettings.AddToAddress.Split(','))
                    {
                        EmailAddress emailAddress = new EmailAddress(toAddress, toAddress);
                        emailMessage.To.Add(emailAddress);
                    }
                }
                if (!string.IsNullOrEmpty(emailSettings.AddCCAddress))
                {
                    foreach (string CCAddress in emailSettings.AddCCAddress.Split(','))
                    {
                        EmailAddress emailAddress = new EmailAddress(CCAddress, CCAddress);
                        emailMessage.CC.Add(emailAddress);
                    }
                }
                if (!string.IsNullOrEmpty(emailSettings.AddBCCAddress))
                {
                    foreach (string BCCAddress in emailSettings.AddBCCAddress.Split(','))
                    {
                        EmailAddress emailAddress = new EmailAddress(BCCAddress);
                        emailMessage.Bcc.Add(emailAddress);
                    }
                }

                //add subject and body prefixes/suffixes
                if (!string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && !string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailSettings.SubjectAddPrefix, emailMessage.Subject, emailSettings.SubjectAddSuffix);
                if (!string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailSettings.SubjectAddPrefix, emailMessage.Subject);
                if (string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && !string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailMessage.Subject, emailSettings.SubjectAddSuffix);
                else emailMessage.Subject = emailMessage.Subject;

                //check if email message body is html or plaintext
                if (emailMessage.IsBodyHtml)
                {
                    if (!string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && !string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.Body = string.Concat(emailSettings.BodyAddPrefix, emailMessage.Body, emailSettings.BodyAddSuffix);
                    if (!string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.Body = string.Concat(emailSettings.BodyAddPrefix, emailMessage.Body);
                    if (string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && !string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.Body = string.Concat(emailMessage.Body, emailSettings.BodyAddSuffix);
                    else emailMessage.Body = emailMessage.Body;
                }
                else
                {
                    if (!string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && !string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.PlainTextBody = string.Concat(emailSettings.BodyAddPrefix, emailMessage.PlainTextBody, emailSettings.BodyAddSuffix);
                    if (!string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.PlainTextBody = string.Concat(emailSettings.BodyAddPrefix, emailMessage.PlainTextBody);
                    if (string.IsNullOrEmpty(emailSettings.BodyAddPrefix) && !string.IsNullOrEmpty(emailSettings.BodyAddSuffix))
                        emailMessage.PlainTextBody = string.Concat(emailMessage.PlainTextBody, emailSettings.BodyAddSuffix);
                    else emailMessage.PlainTextBody = emailMessage.Body;
                }

                //send email
                ISendEmailChore sendEmailChore = null;

                if (existingAccount != null)
                {
                    if (existingAccount.Provider == "SMTP")
                        sendEmailChore = new SmtpSendEmailChore(existingAccount, emailSettings);
                    else if (existingAccount.Provider == "Azure")
                        sendEmailChore = new AzureSendEmailChore(emailSettings, existingAccount);
                }

                if (sendEmailChore != null)
                {
                    try
                    {
                        if (email.Status != StatusType.Blocked.ToString() || email.Status != StatusType.Failed.ToString())
                        {
                            sendEmailChore.SendEmail(emailMessage);
                            email.Status = StatusType.Sent.ToString();
                            email.Reason = "Email was sent successfully.";
                        }
                    }
                    catch (Exception ex)
                    {
                        email.Status = StatusType.Failed.ToString();
                        email.Reason = "Error: " + ex.Message;
                    }
                }
                else
                {
                    email.Status = StatusType.Failed.ToString();
                    email.Reason = "Email failed to send.";
                }
            }

            //log email and its status
            if (existingAccount != null)
                email.EmailAccountId = Guid.Parse(existingAccount.Id.ToString());
            email.SentOnUTC = DateTime.UtcNow;
            string newEmailMessage = Regex.Replace(emailMessage.Body, @"(<sensitive(\s|\S)*?<\/sensitive>)", "NULL");
            email.EmailObjectJson = newEmailMessage;
            List<string> nameList = new List<string>();
            List<string> emailList = new List<string>();
            foreach (EmailAddress address in emailMessage.From)
            {
                nameList.Add(address.Name);
                emailList.Add(address.Address);
            }
            email.SenderName = JsonConvert.SerializeObject(nameList);
            email.SenderAddress = JsonConvert.SerializeObject(emailList);
            email.SenderUserId = _applicationUser?.PersonId;
            if (string.IsNullOrEmpty(direction))
                email.Direction = Direction.Unknown.ToString();
            else email.Direction = direction;

            //TODO: add logic to next two lines of code to allow for assignment of these Guids
            email.ConversationId = null;
            email.ReplyToEmailId = null;

            if (emailObject.Status == StatusType.Unknown.ToString())
            {
                email.Id = emailObject.Id;
                email.CreatedOn = DateTime.UtcNow;
                email.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                _emailRepository.Add(email);
            }
            else if (email.Id != null && email.Id != emailId)
            {
                email.CreatedOn = DateTime.UtcNow;
                email.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                _emailRepository.Add(email);
            }
            else
            {
                emailObject.EmailAccountId = email.EmailAccountId;
                emailObject.SentOnUTC = email.SentOnUTC;
                emailObject.EmailObjectJson = email.EmailObjectJson;
                emailObject.SenderName = email.SenderName;
                emailObject.SenderAddress = email.SenderAddress;
                emailObject.SenderUserId = email.SenderUserId;
                emailObject.Direction = email.Direction;
                emailObject.ConversationId = email.ConversationId;
                emailObject.ReplyToEmailId = email.ReplyToEmailId;
                emailObject.Status = email.Status;
                emailObject.Reason = email.Reason;
                _emailRepository.Update(emailObject);
            }
            return Task.CompletedTask;
        }

        public bool IsEmailAllowed()
        {
            var organizationId = Guid.Parse(_organizationManager.GetDefaultOrganization()?.Id?.ToString());
            var emailSettings = _emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();
            var existingAccount = _emailAccountRepository.Find(null, s => s.IsDefault)?.Items?.FirstOrDefault();

            if (emailSettings == null || existingAccount == null)
                return false;
            else if (emailSettings.IsEmailDisabled || existingAccount.IsDisabled)
                return false;
            else if (organizationId.Equals(Guid.Empty))
                return false;
            else return true;
        }

        public PaginatedList<AllEmailAttachmentsViewModel> GetEmailAttachmentsAndNames(Guid emailId, Predicate<AllEmailAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _emailAttachmentRepository.FindAllView(emailId, predicate, sortColumn, direction, skip, take);
        }

        public List<EmailAttachment> AddFileAttachments(string emailId, string[] requests, string driveId = null)
        {
            if (requests.Length == 0 || requests == null) throw new EntityOperationException("No files found to attach");

            var drive = GetDrive(driveId);
            driveId = drive.Id.ToString();

            var emailAttachments = new List<EmailAttachment>();
            var files = new List<FileFolderViewModel>();

            foreach (var request in requests)
            {
                var file = _fileManager.ExportFileFolder(request, driveId).Result;
                if (file == null) throw new EntityDoesNotExistException("File could not be found");

                long? size = file.Size;
                if (size <= 0) throw new EntityOperationException($"File size of file {file.Name} cannot be 0");

                //create email attachment file under email id folder
                var fileToCheck = _storageFileRepository.GetOne(file.Id.Value);
                var orgId = _organizationManager.GetDefaultOrganization().Id.ToString();
                using (var stream = IOFile.OpenRead(fileToCheck.StorageLocation))
                {
                    file.Files = new IFormFile[] { new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)) };
                };
                var path = Path.Combine(driveId, "Email Attachments", emailId);
                file.StoragePath = path;
                file.FullStoragePath = path;

                CheckStoragePathExists(file, 0, Guid.Parse(emailId), driveId, drive.Name);
                file = _fileManager.AddFileFolder(file, driveId)[0];
                files.Add(file);

                //create email attachment
                EmailAttachment emailAttachment = new EmailAttachment()
                {
                    Name = file.Name,
                    FileId = file.Id,
                    ContentType = file.ContentType,
                    ContentStorageAddress = file.FullStoragePath,
                    SizeInBytes = file.Size,
                    EmailId = Guid.Parse(emailId),
                    CreatedBy = _applicationUser?.UserName,
                    CreatedOn = DateTime.UtcNow
                };
                _emailAttachmentRepository.Add(emailAttachment);
                emailAttachments.Add(emailAttachment);
            }

            _fileManager.AddBytesToFoldersAndDrive(files);
            
            return emailAttachments;
        }

        public EmailAttachment UpdateAttachment(string id, UpdateEmailAttachmentViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingAttachment = _emailAttachmentRepository.GetOne(entityId);
            if (existingAttachment == null) throw new EntityOperationException("No file found to update");

            request.DriveId = CheckDriveId(request.DriveId);
            var drive = _storageDriveRepository.GetOne(Guid.Parse(request.DriveId));

            var file = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), request.DriveId, "Files");

            if (file == null) throw new EntityDoesNotExistException($"File could not be found");

            long? size = file.Size;
            if (size <= 0) throw new EntityOperationException($"File size of file {file.Name} cannot be 0");

            //update email attachment entity
            var formFile = request.File;
            string storagePath = Path.Combine(drive.Name, "Email Attachments", formFile.FileName);
            if (!string.IsNullOrEmpty(formFile.FileName))
                existingAttachment.Name = formFile.FileName;

            existingAttachment.ContentType = formFile.ContentType;
            existingAttachment.SizeInBytes = formFile.Length;
            existingAttachment.ContentStorageAddress = storagePath;
            _emailAttachmentRepository.Update(existingAttachment);

            //update file entity and file
            file.Files = new IFormFile[] { formFile };
            file.StoragePath = storagePath;
            var fileView = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), request.DriveId, "Files");
            file.FullStoragePath = fileView.FullStoragePath;
            _fileManager.UpdateFile(file);

            return existingAttachment;
        }

        public void DeleteAll(string emailId, string driveId)
        {
            var attachments = _emailAttachmentRepository.Find(null, q => q.EmailId == Guid.Parse(emailId))?.Items;
            if (attachments.Count != 0)
            {
                if (string.IsNullOrEmpty(driveId))
                {
                    var fileToDelete = _storageFileRepository.GetOne(attachments[0].FileId.Value);
                    driveId = fileToDelete.StorageDriveId.ToString();
                }

                var fileView = new FileFolderViewModel();
                foreach (var attachment in attachments)
                {
                    fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveId, "Files");
                    _emailAttachmentRepository.SoftDelete(attachment.Id.Value);
                }
                var folder = _fileManager.GetFileFolder(fileView.ParentId.ToString(), driveId, "Folders");
                if (!folder.HasChild.Value)
                    _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
                else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { fileView });
            }
            else throw new EntityDoesNotExistException("No attachments found to delete");
        }

        public void DeleteOne(string id, string driveId)
        {
            var attachment = _emailAttachmentRepository.Find(null, q => q.Id == Guid.Parse(id))?.Items?.FirstOrDefault();
            if (attachment != null)
            {
                if (string.IsNullOrEmpty(driveId))
                {
                    var fileToDelete = _storageFileRepository.GetOne(attachment.FileId.Value);
                    driveId = fileToDelete.StorageDriveId.ToString();
                }
                var fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveId, "Files");
                var folder = _fileManager.GetFileFolder(fileView.ParentId.ToString(), driveId, "Folders");
                if (!folder.HasChild.Value)
                    _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
                else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { fileView });
            }
            else throw new EntityDoesNotExistException("Attachment could not be found");
        }

        public EmailViewModel SendDraftEmail(string id, SendEmailViewModel request, string emailAccountName)
        {
            EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(request.EmailMessageJson);

            Guid emailId = Guid.Parse(id);
            var email = _emailRepository.GetOne(emailId);

            var emailAttachments = new List<EmailAttachment>();
            var attachments = _emailAttachmentRepository.Find(null, q => q.EmailId == emailId)?.Items;

            if (email.Status.Equals("Draft"))
            {
                //if file doesn't exist in files: add file entity, upload file, and add email attachment entity
                string hash = string.Empty;
                if (request.Files == null || request.Files.Length == 0)
                    emailMessage.Attachments = attachments;
                else
                {
                    IFormFile[] filesArray = CheckFiles(request.Files, hash, attachments, request.DriveId);
                    emailAttachments = AddAttachments(filesArray, emailId, hash);
                    emailMessage.Attachments = emailAttachments;
                }

                //email account name is nullable, so it needs to be used as a query parameter instead of in the put url
                //if no email account is chosen, the default organization account will be used
                SendEmailAsync(emailMessage, emailAccountName, id, "Outgoing");

                email = _emailRepository.Find(null, q => q.Id == emailId)?.Items?.FirstOrDefault();
                EmailViewModel emailViewModel = GetEmailViewModel(email, attachments);
                if (attachments.Count == 0 || attachments == null)
                    emailViewModel.Attachments = emailAttachments;
                return emailViewModel;
            }
            else throw new EntityOperationException("Email was not sent because it is not a draft");
        }

        public EmailViewModel SendNewEmail(SendEmailViewModel request, string emailAccountName)
        {
            EmailMessage emailMessage = JsonConvert.DeserializeObject<EmailMessage>(request.EmailMessageJson);
            Guid id = Guid.NewGuid();
            List<EmailAttachment> attachments = new List<EmailAttachment>();

            if (request.Files != null)
            {
                request.DriveId = CheckDriveId(request.DriveId);

                //create email attachment entities for each file attachment
                attachments = AddAttachments(request.Files, id, request.DriveId);
                //add attachment entities to email message
                emailMessage.Attachments = attachments;
            }

            SendEmailAsync(emailMessage, emailAccountName, id.ToString(), "Outgoing");

            Email email = _emailRepository.Find(null, q => q.Id == id)?.Items?.FirstOrDefault();
            EmailViewModel emailViewModel = GetEmailViewModel(email, attachments);
            return emailViewModel;
        }

        public EmailViewModel UpdateFiles(string id, UpdateEmailViewModel request)
        {
            var email = _emailRepository.GetOne(Guid.Parse(id));
            if (email == null) throw new EntityDoesNotExistException("Email could not be found or does not exist");

            email.ConversationId = request.ConversationId;
            email.Direction = request.Direction;
            email.EmailObjectJson = request.EmailObjectJson;
            email.SenderAddress = request.SenderAddress;
            email.SenderName = request.SenderName;
            email.SenderUserId = _applicationUser?.PersonId;
            email.Status = request.Status;
            email.EmailAccountId = request.EmailAccountId;
            email.ReplyToEmailId = request.ReplyToEmailId;
            email.Reason = request.Reason;
            email.SentOnUTC = request.SentOnUTC;

            //if files don't exist in file manager: add file entity, upload file, and add email attachment attachment entity
            var attachments = _emailAttachmentRepository.Find(null, q => q.EmailId == Guid.Parse(id))?.Items;
            if (string.IsNullOrEmpty(request.DriveId))
            {
                var fileToCheck = _storageFileRepository.GetOne(attachments[0].Id.Value);
                var drive = _storageDriveRepository.GetOne(fileToCheck.StorageDriveId.Value);
                request.DriveId = drive.Id.ToString();
            }

            string hash = string.Empty;
            IFormFile[] filesArray = CheckFiles(request.Files, hash, attachments, request.DriveId);
            var emailAttachments = AddAttachments(filesArray, email.Id.Value, request.DriveId);

            //update email
            _emailRepository.Update(email);

            attachments = _emailAttachmentRepository.Find(null, q => q.EmailId == Guid.Parse(id))?.Items;
            EmailViewModel response = GetEmailViewModel(email, attachments);
            if (attachments.Count == 0 || attachments == null)
                response.Attachments = emailAttachments;

            return response;
        }

        public EmailViewModel GetEmailView(EmailViewModel emailViewModel)
        {
            var attachmentsList = _emailAttachmentRepository.Find(null, q => q.EmailId == emailViewModel.Id)?.Items;
            if (attachmentsList != null)
                emailViewModel.Attachments = attachmentsList;
            else emailViewModel.Attachments = null;

            return emailViewModel;
        }

        public async Task<FileFolderViewModel> Export(string id, string driveId)
        {
            Guid attachmentId;
            Guid.TryParse(id, out attachmentId);

            EmailAttachment attachment = _emailAttachmentRepository.GetOne(attachmentId);
            if (attachment == null || attachment.FileId == null || attachment.FileId == Guid.Empty)
                throw new EntityDoesNotExistException($"Email attachment with id {id} could not be found or doesn't exist");

            driveId = CheckDriveIdByFileId(attachment.FileId.ToString(), driveId);

            var response = await _fileManager.ExportFileFolder(attachment.FileId.ToString(), driveId);
            return response;
        }

        public enum StatusType : int
        {
            Failed = 0,
            Sent = 1,
            Blocked = 3,
            Draft = 4,
            Unknown = 5
        }

        public enum Direction : int
        {
            Outgoing = 0,
            Incoming = 1,
            Unknown = 2
        }

        private string CheckDriveId(string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();
                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
                else
                    driveId = drive.Id.ToString();
            }
            return driveId;
        }

        private FileFolderViewModel CheckStoragePathExists(FileFolderViewModel view, long? size, Guid? id, string driveId, string driveName)
        {
            //check if storage path exists; if it doesn't exist, create folder
            var folder = _fileManager.GetFileFolderByStoragePath(view.FullStoragePath, driveName);
            if (folder.Name == null)
            {
                folder.Name = id.ToString();
                folder.StoragePath = Path.Combine(driveName, "Email Attachments");
                folder.IsFile = false;
                folder.Size = size;
                folder = _fileManager.AddFileFolder(folder, driveId)[0];
            }
            return folder;
        }

        private IFormFile[] CheckFiles(IFormFile[] files, string hash, List<EmailAttachment> attachments, string driveId)
        {
            if (files != null)
            {
                var filesList = files.ToList();
                
                if (string.IsNullOrEmpty(driveId))
                {
                    var fileToCheck = _storageFileRepository.GetOne(attachments[0].FileId.Value);
                    var drive = _storageDriveRepository.GetOne(fileToCheck.StorageDriveId.Value);
                    driveId = drive.Id.ToString();
                }

                foreach (var attachment in attachments)
                {
                    var fileView = _fileManager.GetFileFolder(attachment.FileId.ToString(), driveId, "Files");
                    var originalHash = fileView.Hash;

                    //check if file with same hash and email id already exists
                    foreach (var file in files)
                    {
                        hash = GetHash(hash, file);

                        //if email attachment already exists and hash is the same: remove from files list
                        if (fileView.ContentType == file.ContentType && originalHash == hash && fileView.Size == file.Length)
                            filesList.Remove(file);

                        //if email attachment exists but the hash is not the same: update the attachment and file, remove from files list
                        else if (fileView.ContentType == file.ContentType && fileView.Name == file.FileName)
                        {
                            fileView = new FileFolderViewModel()
                            {
                                ContentType = file.ContentType,
                                Files = new IFormFile[] { file },
                                IsFile = true,
                                StoragePath = fileView.StoragePath,
                                Name = file.FileName,
                                Id = fileView.Id
                            };
                            attachment.SizeInBytes = file.Length;
                            _emailAttachmentRepository.Update(attachment);
                            _fileManager.UpdateFile(fileView);
                            filesList.Remove(file);
                        }
                    }
                }
                //if file doesn't exist, keep it in files list and return files to be attached
                var filesArray = filesList.ToArray();
                return filesArray;
            }
            else
                return Array.Empty<IFormFile>();
        }

        private string GetHash(string hash, IFormFile file)
        {
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream())
            {
                file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                HashAlgorithm hashAlgorithm = sha256Hash;
                byte[] data = hashAlgorithm.ComputeHash(bytes);
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                hash = sBuilder.ToString();
            }
            return hash;
        }

        private StorageDrive GetDrive(string driveId)
        {
            var drive = _storageDriveRepository.GetOne(Guid.Parse(driveId));
            if (drive == null)
            {
                drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();

                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
            }
            return drive;
        }

        private string CheckDriveIdByFileId(string id, string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var fileToExport = _storageFileRepository.GetOne(Guid.Parse(id));
                driveId = fileToExport.StorageDriveId.ToString();
            }
            return driveId;
        }
    }
}