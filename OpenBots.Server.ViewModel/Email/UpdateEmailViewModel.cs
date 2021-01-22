using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel.Email
{
    public class UpdateEmailViewModel : IViewModel<Model.Configuration.Email, UpdateEmailViewModel>
    {

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
        public Guid? EmailAccountId { get; set; }
        public IFormFile[]? Files { get; set; }

        public UpdateEmailViewModel Map(Model.Configuration.Email entity)
        {
            UpdateEmailViewModel emailViewModel = new UpdateEmailViewModel()
            {
                ConversationId = entity.ConversationId,
                EmailAccountId = entity.EmailAccountId,
                ReplyToEmailId = entity.ReplyToEmailId,
                SenderUserId = entity.SenderUserId,
                Direction = entity.Direction,
                EmailObjectJson = entity.EmailObjectJson,
                SentOnUTC = entity.SentOnUTC,
                Reason = entity.Reason,
                SenderAddress = entity.SenderAddress,
                SenderName = entity.SenderName,
                Status = entity.Status,
            };

            return emailViewModel;
        }
    }
}
