using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using IOFile = System.IO.File;

namespace OpenBots.Server.Model.Core
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            To = new List<EmailAddress>();
            CC = new List<EmailAddress>();
            Bcc = new List<EmailAddress>();
            From = new List<EmailAddress>();
            ReplyToList = new List<EmailAddress>();
            Headers = new List<EmailHeader>();
            Attachments = new List<EmailAttachment>();
            IsBodyHtml = true;
        }

        public static MimeMessage FromStub(EmailMessage msg)
        {
            MimeMessage outMsg = new MimeMessage();

            var from = msg.From.FirstOrDefault();
            outMsg.From.Add(from.ToMailAddress());

            EmailAddress.IterateBack(msg.To).ForEach(addr => outMsg.To.Add(addr));
            if (msg.CC != null && msg.CC.Count != 0)
                if (!string.IsNullOrEmpty(msg.CC[0].Name) && !string.IsNullOrEmpty(msg.CC[0].Address))
                    EmailAddress.IterateBack(msg.CC).ForEach(addr => outMsg.Cc.Add(addr));
            if (msg.Bcc != null && msg.Bcc.Count != 0)
                if (!string.IsNullOrEmpty(msg.Bcc[0].Name) && !string.IsNullOrEmpty(msg.Bcc[0].Address))
                    EmailAddress.IterateBack(msg.Bcc).ForEach(addr => outMsg.Bcc.Add(addr));
            outMsg.Subject = msg.Subject;

            var body = new TextPart();
            if (msg.IsBodyHtml)
            {
                body = new TextPart("html")
                {
                    Text = msg.Body
                };
            }
            else
            {
                body = new TextPart("plain")
                {
                    Text = msg.PlainTextBody
                };
            }

            //create the multipart/mixed container to hold the message text and the attachment
            var multipart = new Multipart("mixed");

            if (msg.Attachments != null || msg.Attachments.Count != 0)
            {
                //get all attachments from email message
                foreach (var msgAttachment in msg.Attachments)
                {
                    var attachment = new MimePart("image", "gif")
                    {
                        Content = new MimeContent(IOFile.OpenRead(msgAttachment.ContentStorageAddress), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = msgAttachment.Name
                    };

                    multipart.Add(attachment);
                }
            }

            multipart.Add(body);

            //set the multipart/mixed as the message body
            outMsg.Body = multipart;

            return outMsg;
        }

        public string? MessageID { get; set; }
        public string? InReplyToMessageID { get; set; }
        public string? MessageTopic { get; set; }
        public DateTime? ReceivedOnUTC { get; set; }
        public List<EmailAddress?>? From { get; set; }
        public EmailAddress Sender { get; set; }
        public List<EmailAddress> To { get; set; }
        public List<EmailAddress?>? CC { get; set; }
        public List<EmailAddress?>? Bcc { get; set; }
        public List<EmailAddress?>? ReplyToList { get; set; }
        public string? Source { get; set; }
        public bool? IsPossibleSpam { get; set; }
        public bool? IsPossibleVirus { get; set; }
        public int? Priority { get; set; }
        public string Subject { get; set; }
        public string? PlainTextBody { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public string? IsBodyContentStored { get; set; }
        public string? BodyContentStorageAddress { get; set; }
        public List<EmailHeader?>? Headers { get; set; }
        public int DeliveryNotificationOptions { get; set; }
        public List<EmailAttachment?>? Attachments { get; set; }
    }
}
