using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Model.Core
{
    /// <summary>
    /// MailMessageStub is a simple serializable Stub for a Email message
    /// This would serve as a translation between various formats of mail messages 
    /// eg.: Aspose, System.Net, Amazon SES, Outlook etc.
    /// It can be serialized to a JSON object and can be stored in Entity or Blob Storage
    /// </summary>
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
        }


        public static System.Net.Mail.MailMessage FromStub(EmailMessage msg)
        {
            System.Net.Mail.MailMessage outMsg = new System.Net.Mail.MailMessage();

            var from = msg.From.FirstOrDefault();

            outMsg.From = from.ToMailAddress();
            EmailAddress.IterateBack(msg.To).ForEach(addr => outMsg.To.Add(addr));
            EmailAddress.IterateBack(msg.CC).ForEach(addr => outMsg.CC.Add(addr));
            EmailAddress.IterateBack(msg.Bcc).ForEach(addr => outMsg.Bcc.Add(addr));
            outMsg.Subject = msg.Subject;
            outMsg.IsBodyHtml = msg.IsBodyHtml;
            outMsg.Body = msg.Body;

            return outMsg;
        }

        public string MessageID { get; set; }
        public string InReplyToMessageID { get; set; }
        public string MessageTopic { get; set; }
        public DateTime RecievedOnUTC { get; set; }
        public List<EmailAddress> From { get; set; }
        public EmailAddress Sender { get; set; }
        public List<EmailAddress> To { get; set; }
        public List<EmailAddress> CC { get; set; }
        public List<EmailAddress> Bcc { get; set; }
        public List<EmailAddress> ReplyToList { get; set; }
        public string Source { get; set; }
        public bool IsPossibleSpam { get; set; }
        public bool IsPossibleVirus { get; set; }
        public int Priority { get; set; }
        public string Subject { get; set; }
        public string PlainTextBody { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public string IsBodyContentStored { get; set; }
        public string BodyContentStorageAddress { get; set; }
        public List<EmailHeader> Headers { get; set; }
        public int DeliveryNotificationOptions { get; set; }
        public List<EmailAttachment> Attachments { get; set; }
    }
}
