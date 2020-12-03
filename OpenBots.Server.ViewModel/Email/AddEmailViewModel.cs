using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel.Email
{
    public class AddEmailViewModel : IViewModel<EmailModel, AddEmailViewModel>
    {
        public Guid? EmailAccountId { get; set; }
        public DateTime? SentOnUTC { get; set; }
        public string? EmailObjectJson { get; set; }
        public string? SenderAddress { get; set; }
        public Guid? SenderUserId { get; set; }
        public string? Status { get; set; }
        public string? Direction { get; set; }
        public IFormFile[]? Files { get; set; }

        public AddEmailViewModel Map(EmailModel entity)
        {
            AddEmailViewModel emailViewModel = new AddEmailViewModel();
            emailViewModel.EmailAccountId = entity.EmailAccountId;
            emailViewModel.SentOnUTC = entity.SentOnUTC;
            emailViewModel.EmailObjectJson = entity.EmailObjectJson;
            emailViewModel.SenderAddress = entity.SenderAddress;
            emailViewModel.SenderUserId = entity.SenderUserId;
            emailViewModel.Status = entity.Status;

            return emailViewModel;
        }
    }
}
