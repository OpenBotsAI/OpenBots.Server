using System;

namespace OpenBots.Server.Model.Core
{
    public class EmailAttachment : NamedEntity
    {
        public EmailAttachment()
        {
        }

        public string ContentType { get; set; }
        public long? SizeInBytes { get; set; }

        /// <summary>
        /// Address where Content of the Attachment is Stored
        /// </summary>
        public string ContentStorageAddress { get; set; }
        public Guid? BinaryObjectId { get; set; }
        public Guid EmailLogId { get; set; }
    }
}
