using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel.Email
{
    public class EmailViewModel : Entity, IViewModel<Model.Configuration.Email, EmailViewModel>
    {
        public Guid? EmailAccountId { get; set; }
        public DateTime? SentOnUTC { get; set; }
        public string? EmailObjectJson { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAddress { get; set; }
        public Guid? SenderUserId { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
        public string Direction { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? ReplyToEmailId { get; set; }
        public List<EmailAttachment>? Attachments { get; set; }

        public EmailViewModel Map(Model.Configuration.Email entity)
        {
            EmailViewModel emailViewModel = new EmailViewModel()
            {
                Id = entity.Id,
                IsDeleted = entity.IsDeleted,
                ConversationId = entity.ConversationId,
                EmailAccountId = entity.EmailAccountId,
                ReplyToEmailId = entity.ReplyToEmailId,
                SenderUserId = entity.SenderUserId,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                DeletedBy = entity.DeletedBy,
                DeleteOn = entity.DeleteOn,
                Direction = entity.Direction,
                EmailObjectJson = entity.EmailObjectJson,
                UpdatedBy = entity.UpdatedBy,
                UpdatedOn = entity.UpdatedOn,
                SentOnUTC = entity.SentOnUTC,
                Reason= entity.Reason,
                SenderAddress = entity.SenderAddress,
                SenderName = entity.SenderName,
                Timestamp = entity.Timestamp,
                Status = entity.Status
            };

            return emailViewModel;
        }
    }
}
