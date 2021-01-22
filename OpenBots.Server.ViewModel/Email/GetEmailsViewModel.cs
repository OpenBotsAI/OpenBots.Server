using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class GetEmailsViewModel : IViewModel<Model.Configuration.Email, GetEmailsViewModel>
    {
        public Guid? Id { get; set; }
        public Guid? EmailAccountId { get; set; }
        public DateTime? SentOnUTC { get; set; }
        public string EmailObjectJson { get; set; }
        public string SenderAddress { get; set; }
        public Guid? SenderUserId { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedOn { get; set; }

        public GetEmailsViewModel Map(Model.Configuration.Email entity)
        {
            GetEmailsViewModel emailViewModel = new GetEmailsViewModel();

            emailViewModel.Id = entity.Id;
            emailViewModel.EmailAccountId = entity.EmailAccountId;
            emailViewModel.SentOnUTC = entity.SentOnUTC;
            emailViewModel.EmailObjectJson = entity.EmailObjectJson;
            emailViewModel.SenderAddress = entity.SenderAddress;
            emailViewModel.SenderUserId = entity.SenderUserId;
            emailViewModel.Status = entity.Status;
            emailViewModel.CreatedOn = DateTime.UtcNow;

            return emailViewModel;
        }
    }
}
