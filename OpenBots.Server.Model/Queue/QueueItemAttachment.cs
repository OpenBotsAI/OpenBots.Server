using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    public class QueueItemAttachment : Entity
    {
        public Guid QueueItemId { get; set; }
        public Guid FileId { get; set; }
        public long SizeInBytes { get; set; }
    }
}
