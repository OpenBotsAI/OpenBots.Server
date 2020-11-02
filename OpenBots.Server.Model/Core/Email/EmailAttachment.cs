namespace OpenBots.Server.Model.Core
{
    public class EmailAttachment
    {
        public EmailAttachment()
        {
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public double SizeInBytes { get; set; }
        public string FileName { get; set; }
        public string IsContentStored { get; set; }

        /// <summary>
        /// Address where Content of the Attachment is Stored
        /// </summary>
        /// <seealso cref="ContentStorageAddress"/>
        public string ContentStorageAddress { get; set; }
        public byte[] Content { get; set; }
    }
}
