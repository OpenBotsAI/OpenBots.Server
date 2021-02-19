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
        IFormFile[] CheckFiles(IFormFile[] files, string hash, List<EmailAttachment> attachments, string driveName = null);
        PaginatedList<AllEmailAttachmentsViewModel> GetEmailAttachmentsAndNames(Guid emailId, Predicate<AllEmailAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        List<EmailAttachment> AddFileAttachments(string emailId, string[] requests, string driveName = null);
        List<EmailAttachment> AddNewAttachments(string emailId, IFormFile[] files, string driveName = null);
        EmailAttachment UpdateAttachment(string id, UpdateEmailAttachmentViewModel request);
        void DeleteAll(string emailId, string driveName = null);
        void DeleteOne(string id, string driveName = null);
        EmailViewModel SendDraftEmail(string id, SendEmailViewModel request, string emailAccountName = null);
        EmailViewModel SendNewEmail(SendEmailViewModel request, string emailAccountName = null);
        EmailViewModel UpdateFiles(string id, UpdateEmailViewModel request);
        void DeleteEmailAttachments(string id, string driveName);
        EmailViewModel GetEmailView(EmailViewModel email);
    }
}