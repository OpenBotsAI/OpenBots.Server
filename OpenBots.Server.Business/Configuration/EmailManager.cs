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
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.Business
{
    public class EmailManager : BaseManager, IEmailManager
    {
        protected IPersonRepository personRepo;
        protected IPersonEmailRepository personEmailRepository;
        protected IEmailAccountRepository emailAccountRepository;
        protected IEmailRepository emailRepository;
        protected IEmailSettingsRepository emailSettingsRepository;
        protected ApplicationUser applicationUser { get; set; }
        protected IOrganizationManager organizationManager;
        protected IHttpContextAccessor httpContextAccessor; 

        public EmailManager(
            IPersonRepository personRepo,
            IPersonEmailRepository personEmailRepository,
            IEmailAccountRepository emailAccountRepository,
            IEmailRepository emailRepository,
            IEmailSettingsRepository emailSettingsRepository,
            IOrganizationManager organizationManager,
            IHttpContextAccessor httpContextAccessor)
        {
            this.personRepo = personRepo;
            this.personEmailRepository = personEmailRepository;
            this.emailAccountRepository = emailAccountRepository;
            this.emailRepository = emailRepository;
            this.emailSettingsRepository = emailSettingsRepository;
            this.organizationManager = organizationManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            personRepo.SetContext(userSecurityContext);
            personEmailRepository.SetContext(userSecurityContext);
            emailAccountRepository.SetContext(userSecurityContext);
            emailRepository.SetContext(userSecurityContext);
            emailSettingsRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Task SendEmailAsync(EmailMessage emailMessage, string accountName = null, string id = null, string direction = null)
        {
            EmailModel emailObject = new EmailModel();
            if (!string.IsNullOrEmpty(id))
            {
                emailObject = emailRepository.Find(null, q => q.Id == Guid.Parse(id))?.Items?.FirstOrDefault();
                if (emailObject == null)
                {
                    emailObject = new EmailModel()
                    {
                        Id = Guid.Parse(id),
                        Status = StatusType.Unknown.ToString()
                    };
                }
            }

            EmailModel email = new EmailModel();
            if (id != null || Guid.Parse(id) != Guid.Empty)
                email.Id = Guid.Parse(id);

            //Find Email Settings and determine is email is enabled/disabled
            var organizationId = Guid.Parse(organizationManager.GetDefaultOrganization().Id.ToString());
            var emailSettings = emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();
            //Check if accountName exists
            var existingAccount = emailAccountRepository.Find(null, d => d.Name.ToLower(null) == accountName?.ToLower(null))?.Items?.FirstOrDefault();
            if (existingAccount == null)
                existingAccount = emailAccountRepository.Find(null, d => d.IsDefault && !d.IsDisabled).Items.FirstOrDefault();

            //If there are NO records in the Email Settings table for that Organization, email should be disabled
            if (emailSettings == null)
            {
                email.Status = StatusType.Blocked.ToString();
                email.Reason = "Email disabled.  Please configure email settings.";
            }
            //If there are email settings but they are disabled, don't send email
            else if (emailSettings != null && emailSettings.IsEmailDisabled)
            {
                email.Status = StatusType.Blocked.ToString();
                email.Reason = "Email functionality has been disabled.";
            }
            else
            {
                if (existingAccount == null && emailSettings != null)
                {
                    existingAccount = emailAccountRepository.Find(null, a => a.IsDefault == true && a.IsDisabled == false)?.Items?.FirstOrDefault();
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
                //Set From Email Address
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

                //Remove email addresses in to, cc, and bcc lists with domains that are blocked or not allowed
                List<EmailAddress> toList = new List<EmailAddress>();
                List<EmailAddress> ccList = new List<EmailAddress>();
                List<EmailAddress> bccList = new List<EmailAddress>();

                if (string.IsNullOrEmpty(emailSettings.AllowedDomains))
                {
                    if (!string.IsNullOrEmpty(emailSettings.BlockedDomains))
                    {
                        //Remove any email address that is in blocked domain
                        IEnumerable<string>? denyList = (new List<string>(emailSettings?.BlockedDomains?.Split(','))).Select(s => s.ToLowerInvariant().Trim());
                        foreach (EmailAddress address in emailMessage.To)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Host.ToLowerInvariant()))
                                    toList.Add(address);
                            }
                        }
                        emailMessage.To.Clear();
                        emailMessage.To = toList;

                        foreach (EmailAddress address in emailMessage.CC)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Host.ToLowerInvariant()))
                                    ccList.Add(address);
                            }
                        }
                        emailMessage.CC.Clear();
                        emailMessage.CC = ccList;

                        foreach (EmailAddress address in emailMessage.Bcc)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (!denyList.Contains(mailAddress.Host.ToLowerInvariant()))
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
                        //Remove any email address that is not on white list
                        IEnumerable<string> allowList = (new List<string>(emailSettings.AllowedDomains.Split(','))).Select(s => s.ToLowerInvariant().Trim());
                        foreach (EmailAddress address in emailMessage.To)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (allowList.Contains(mailAddress.Host.ToLowerInvariant()))
                                    toList.Add(address);
                            }
                        }
                        emailMessage.To.Clear();
                        emailMessage.To = toList;

                        foreach (EmailAddress address in emailMessage.CC)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (allowList.Contains(mailAddress.Host.ToLowerInvariant()))
                                    ccList.Add(address);
                            }
                        }
                        emailMessage.CC.Clear();
                        emailMessage.CC = ccList;

                        foreach (EmailAddress address in emailMessage.Bcc)
                        {
                            if (!string.IsNullOrEmpty(address.Address))
                            {
                                MailAddress mailAddress = new MailAddress(address.Address);
                                if (allowList.Contains(mailAddress.Host.ToLowerInvariant()))
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

                //Add any necessary additional email addresses (Administrators, etc.)
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

                //Add Subject and Body Prefixes/Suffixes
                if (!string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && !string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailSettings.SubjectAddPrefix, emailMessage.Subject, emailSettings.SubjectAddSuffix);
                if (!string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailSettings.SubjectAddPrefix, emailMessage.Subject);
                if (string.IsNullOrEmpty(emailSettings.SubjectAddPrefix) && !string.IsNullOrEmpty(emailSettings.SubjectAddSuffix))
                    emailMessage.Subject = string.Concat(emailMessage.Subject, emailSettings.SubjectAddSuffix);
                else emailMessage.Subject = emailMessage.Subject;

                //Check if Email Message body is html or plaintext
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

                //Send email
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

            //Log email and its status
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
            email.SenderUserId = applicationUser?.PersonId;
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
                email.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                emailRepository.Add(email);
            }
            else if (email.Id != null && email.Id != Guid.Parse(id))
            {
                email.CreatedOn = DateTime.UtcNow;
                email.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                emailRepository.Add(email);
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
                emailRepository.Update(emailObject);
            }
            return Task.CompletedTask;
        }

        public bool IsEmailAllowed()
        {
            var organizationId = Guid.Parse(organizationManager.GetDefaultOrganization()?.Id?.ToString());
            var emailSettings = emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();
            var existingAccount = emailAccountRepository.Find(null, s => s.IsDefault)?.Items?.FirstOrDefault();

            if (emailSettings == null || existingAccount == null)
                return false;
            else if (emailSettings.IsEmailDisabled || existingAccount.IsDisabled)
                return false;
            else if (organizationId.Equals(Guid.Empty))
                return false;
            else return true;
        }

        public enum StatusType : int
        {
            Failed = 0,
            Sent = 1,
            Blocked = 3,
            Unknown = 4
        }

        public enum Direction : int
        {
            Outgoing = 0,
            Incoming = 1,
            Unknown = 2
        }
    }
}
