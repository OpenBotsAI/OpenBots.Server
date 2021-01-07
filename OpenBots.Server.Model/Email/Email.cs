using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.Configuration
{
    public class Email : Entity
    {
        public Guid? EmailAccountId { get; set; }
        public DateTime? SentOnUTC { get; set; }
        public string? EmailObjectJson { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAddress { get; set; }
        public Guid? SenderUserId { get; set; }
        public string Status { get; set; }
        public string? Reason { get; set; }
        public string Direction { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? ReplyToEmailId { get; set; }
    }
}