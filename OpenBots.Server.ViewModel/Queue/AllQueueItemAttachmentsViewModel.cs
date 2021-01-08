using System;

namespace OpenBots.Server.ViewModel.Queue
{
    public class AllQueueItemAttachmentsViewModel
    {
        public Guid QueueItemId { get; set; }
        public Guid BinaryObjectId { get; set; }
        public long SizeInBytes { get; set; }
        public string Name { get; set; }
    }
}
