using Microsoft.AspNetCore.Http;
using System;

namespace OpenBots.Server.ViewModel.Email
{
    public class UpdateEmailAttachmentViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public long SizeInBytes { get; set; }
        public Guid? BinaryObjectId { get; set; }
        public IFormFile? file { get; set; }
    }
}
