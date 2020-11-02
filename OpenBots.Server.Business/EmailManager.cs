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

namespace OpenBots.Server.Business
{
    public class EmailManager : BaseManager, IEmailManager
    {
        protected IPersonRepository _personRepo;
        protected IPersonEmailRepository _personEmailRepository;
        protected IEmailAccountRepository _emailAccountRepository;
        protected IEmailLogRepository _emailLogRepository;
        protected IEmailSettingsRepository _emailSettingsRepository;
        protected ApplicationUser applicationUser;
        protected IOrganizationManager _organizationManager;

        public EmailManager(
            IPersonRepository personRepo,
            IPersonEmailRepository personEmailRepository,
            IEmailAccountRepository emailAccountRepository,
            IEmailLogRepository emailLogRepository,
            IEmailSettingsRepository emailSettingsRepository,
            IOrganizationManager organizationManager)
        {
            _personRepo = personRepo;
            _personEmailRepository = personEmailRepository;
            _emailAccountRepository = emailAccountRepository;
            _emailLogRepository = emailLogRepository;
            _emailSettingsRepository = emailSettingsRepository;
            _organizationManager = organizationManager;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _personRepo.SetContext(userSecurityContext);
            _personEmailRepository.SetContext(userSecurityContext);
            _emailAccountRepository.SetContext(userSecurityContext);
            _emailLogRepository.SetContext(userSecurityContext);
            _emailSettingsRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Task SendEmailAsync(EmailMessage emailMessage, string accountName = "")
        {
            EmailLog emailLog = new EmailLog();

            //Find Email Settings and determine is email is enabled/disabled
            var organizationId = Guid.Parse(_organizationManager.GetDefaultOrganization().Id.ToString());
            var emailSettings = _emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();

            // If there are NO records in the Email Settings table for that Organization, email should be disabled
            if (emailSettings == null)
            {
                emailLog.Status = StatusType.Blocked.ToString();
                emailLog.Reason = "Email disabled.  Please configure email settings.";
            }

            if (emailSettings != null && emailSettings.IsEmailDisabled)
            {
                emailLog.Status = StatusType.Blocked.ToString();
                emailLog.Reason = "Email functionality has been disabled.";
            }

            //Check if accountName exists
            var existingAccount = _emailAccountRepository.Find(null, d => d.Name.ToLower(null) == accountName.ToLower(null))?.Items?.FirstOrDefault();
            if (existingAccount == null && emailSettings != null)
            {
                existingAccount = _emailAccountRepository.Find(null, a => a.IsDefault == true && a.IsDisabled == false)?.Items?.FirstOrDefault();
                if (existingAccount == null)
                    emailLog.Status = StatusType.Failed.ToString();
                emailLog.Reason = $"Account '{accountName}' could be found.";
                if (existingAccount != null && existingAccount.IsDisabled == true)
                    emailLog.Status = StatusType.Blocked.ToString();
                emailLog.Reason = $"Account '{accountName}' has been disabled.";
            }

            //Set From Email Address
            if (existingAccount != null)
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
            else
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

            if (emailMessage.To.Count == 0)
                emailLog.Status = StatusType.Blocked.ToString();
            emailLog.Reason = "No email addresses to send email to.";

            //Add any necessary additional email addresses (Administrators, etc.)
            if (!string.IsNullOrEmpty(emailSettings.AddToAddress))
            {
                foreach (string toAddress in emailSettings.AddToAddress.Split(','))
                {
                    EmailAddress email = new EmailAddress(toAddress, toAddress);
                    emailMessage.To.Add(email);
                }
            }
            if (!string.IsNullOrEmpty(emailSettings.AddCCAddress))
            {
                foreach (string CCAddress in emailSettings.AddCCAddress.Split(','))
                {
                    EmailAddress email = new EmailAddress(CCAddress, CCAddress);
                    emailMessage.CC.Add(email);
                }
            }
            if (!string.IsNullOrEmpty(emailSettings.AddBCCAddress))
            {
                foreach (string BCCAddress in emailSettings.AddBCCAddress.Split(','))
                {
                    EmailAddress email = new EmailAddress(BCCAddress);
                    emailMessage.Bcc.Add(email);
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
            //emailMessage.IsBodyHtml = true;
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
                    if (emailLog.Status != StatusType.Blocked.ToString() || emailLog.Status != StatusType.Failed.ToString()) ;
                    {
                        sendEmailChore.SendEmail(emailMessage);
                        emailLog.Status = StatusType.Sent.ToString();
                        emailLog.Reason = "Email was sent successfully.";
                    }
                }
                catch (Exception ex)
                {
                    emailLog.Status = StatusType.Failed.ToString();
                    emailLog.Reason = "Error: " + ex.Message;
                }
            }
            else
            {
                emailLog.Status = StatusType.Failed.ToString();
                emailLog.Reason = "Email failed to send.";
            }

            //Log email and its status
            emailLog.Id = Guid.NewGuid();
            if (existingAccount != null)
                emailLog.EmailAccountId = Guid.Parse(existingAccount.Id.ToString());
            emailLog.SentOnUTC = DateTime.UtcNow;
            string newEmailMessage = Regex.Replace(emailMessage.Body, @"(<sensitive(\s|\S)*?<\/sensitive>)", "NULL");
            emailLog.EmailObjectJson = JsonConvert.SerializeObject(newEmailMessage);
            List<string> nameList = new List<string>();
            List<string> emailList = new List<string>();
            foreach (EmailAddress address in emailMessage.From)
            {
                nameList.Add(address.Name);
                emailList.Add(address.Address);
            }
            emailLog.SenderName = JsonConvert.SerializeObject(nameList);
            emailLog.SenderAddress = JsonConvert.SerializeObject(emailList);
            emailLog.CreatedOn = DateTime.UtcNow;
            emailLog.CreatedBy = applicationUser?.UserName;
            emailLog.SenderUserId = applicationUser?.PersonId;
            _emailLogRepository.Add(emailLog);

            return Task.CompletedTask;
        }

        public bool IsEmailAllowed()
        {
            var organizationId = Guid.Parse(_organizationManager.GetDefaultOrganization().Id.ToString());
            var emailSettings = _emailSettingsRepository.Find(null, s => s.OrganizationId == organizationId).Items.FirstOrDefault();
            var existingAccount = _emailAccountRepository.Find(null, s => s.IsDefault)?.Items?.FirstOrDefault();

            if (emailSettings == null || existingAccount == null)
                return false;
            else if (emailSettings.IsEmailDisabled || existingAccount.IsDisabled)
                return false;
            else return true;
        }

        public enum StatusType : int
        {
            Failed = 0,
            Sent = 1,
            Blocked = 3
        }
    }
}
