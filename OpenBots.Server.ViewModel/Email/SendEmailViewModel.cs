using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;

namespace OpenBots.Server.ViewModel.Email
{
    public class SendEmailViewModel : IViewModel<EmailMessage, SendEmailViewModel>
    {
        public string EmailMessageJson { get; set; }
        public IFormFile[]? Files { get; set; }

        public SendEmailViewModel Map(EmailMessage entity)
        {
            SendEmailViewModel emailViewModel = new SendEmailViewModel()
            {

            };

            return emailViewModel;
        }
    }
}
