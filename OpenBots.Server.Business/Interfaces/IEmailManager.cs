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
        EmailViewModel GetEmailViewModel(Email email, List<EmailAttachment> attachments);
        Email CreateEmail(AddEmailViewModel request);
        IFormFile[] CheckFiles(IFormFile[] files, Guid id, string hash, List<EmailAttachment> attachments);
        public PaginatedList<AllEmailAttachmentsViewModel> GetEmailAttachmentsAndNames(Guid emailId, Predicate<AllEmailAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}