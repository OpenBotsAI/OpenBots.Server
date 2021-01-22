using System;

namespace OpenBots.Server.ViewModel.Email
{
    public class AllEmailAttachmentsViewModel
    {
        public Guid EmailId { get; set; }
        public Guid BinaryObjectId { get; set; }
        public long SizeInBytes { get; set; }
        public string Name { get; set; }
    }
}
