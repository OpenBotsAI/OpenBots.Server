using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.ViewModel.Email
{
    public class SendEmailViewModel
    {
        public string? EmailMessageJson { get; set; }
        public IFormFile[]? Files { get; set; }
    }
}
