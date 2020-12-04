using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.Email;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IEmailManager
    {
        Task SendEmailAsync(EmailMessage emailMessage, string accountName = null, string id = null, string direction = null);
        bool IsEmailAllowed();
        List<EmailAttachment> AddAttachments(IFormFile[] files, Guid id, string hash = null);
        EmailViewModel GetEmailViewModel(EmailModel email, List<EmailAttachment> attachments);
        EmailModel CreateEmail(AddEmailViewModel request);
        IFormFile[] CheckFiles(IFormFile[] files, Guid id, string hash, List<EmailAttachment> attachments);
    }
}