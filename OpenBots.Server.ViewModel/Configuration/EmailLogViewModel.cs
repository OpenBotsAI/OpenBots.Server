using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using System;

namespace OpenBots.Server.ViewModel
{
    public class EmailLogViewModel :IViewModel<EmailLog, EmailLogViewModel>
    {
        public Guid? Id { get; set; }
        public Guid EmailAccountId { get; set; }
        public DateTime SentOnUTC { get; set; }
        public string EmailObjectJson { get; set; }
        public string SenderAddress { get; set; }
        public Guid? SenderUserId { get; set; }
        public string Status { get; set; }

        public EmailLogViewModel Map(EmailLog entity)
        {
            EmailLogViewModel emailLogViewModel = new EmailLogViewModel();

            emailLogViewModel.Id = entity.Id;
            emailLogViewModel.EmailAccountId = entity.EmailAccountId;
            emailLogViewModel.SentOnUTC = entity.SentOnUTC;
            emailLogViewModel.EmailObjectJson = entity.EmailObjectJson;
            emailLogViewModel.SenderAddress = entity.SenderAddress;
            emailLogViewModel.SenderUserId = entity.SenderUserId;
            emailLogViewModel.Status = entity.Status;

            return emailLogViewModel;
        }
    }
}
