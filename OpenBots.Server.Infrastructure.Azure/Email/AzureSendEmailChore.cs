using OpenBots.Server.Model.Core;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using OpenBots.Server.Infrastructure.Email;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.Infrastructure.Azure.Email
{
    public class AzureSendEmailChore : BaseSendEmailChore, ISendEmailChore
    {
        public AzureSendEmailChore(EmailSettings setting, EmailAccount emailAccount) : base(setting, emailAccount)
        {
            this.setting = setting; 
        }

        public override void SendEmail(EmailMessage message)
        {
            if (setting.IsEmailDisabled)
                return;
            try
            {
                var sendGridClient = new SendGridClient(emailAccount.ApiKey);
                var sendGridMessage = new SendGridMessage();

                var fromAddr = message.From.FirstOrDefault();
                if (fromAddr != null)
                    sendGridMessage.From = new SendGrid.Helpers.Mail.EmailAddress(fromAddr.Address, fromAddr.Name);

                sendGridMessage.Subject = message.Subject;
                sendGridMessage.PlainTextContent = message.PlainTextBody;
                sendGridMessage.HtmlContent = message.Body;

                foreach (var toAddr in message.To)
                {
                    sendGridMessage.AddTo(new SendGrid.Helpers.Mail.EmailAddress(toAddr.Address, toAddr.Name));
                }
                foreach (var ccAddr in message.CC)
                {
                    sendGridMessage.AddCc(new SendGrid.Helpers.Mail.EmailAddress(ccAddr.Address, ccAddr.Name));
                }
                foreach (var bccAddr in message.Bcc)
                {
                    sendGridMessage.AddBcc(new SendGrid.Helpers.Mail.EmailAddress(bccAddr.Address, bccAddr.Name));
                }

                // Disable click tracking.See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
                sendGridMessage.SetClickTracking(false, false);

                sendGridClient.SendEmailAsync(sendGridMessage);
            }
            catch (Exception ex)
            {
                throw new CannotSendEmailException("Cannot Send Email." + ex.Message, ex);
            }
        }
    }
}
