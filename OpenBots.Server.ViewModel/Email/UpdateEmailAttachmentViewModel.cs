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
        public Guid? FileId { get; set; }
        public IFormFile? File { get; set; }
        public string? DriveName { get; set; }
    }
}
