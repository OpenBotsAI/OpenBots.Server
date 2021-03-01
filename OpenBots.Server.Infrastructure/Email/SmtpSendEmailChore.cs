using MimeKit;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using System;
using MailKit.Net.Smtp;

namespace OpenBots.Server.Infrastructure.Email
{
    public class SmtpSendEmailChore : BaseSendEmailChore, ISendEmailChore
    {
        protected EmailAccount _smtpSetting;

        public SmtpSendEmailChore(EmailAccount smtpSetting, EmailSettings sendEmailSetting) : base(sendEmailSetting, smtpSetting)
        {
            _smtpSetting = smtpSetting;
        }

        public override void SendEmail(EmailMessage message)
        {
            if (setting.IsEmailDisabled)
                return;

            try
            {
                MimeMessage mail = EmailMessage.FromStub(message);

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(_smtpSetting.Host, _smtpSetting.Port);
                    client.Authenticate(_smtpSetting.Username, _smtpSetting.EncryptedPassword);
                    client.Send(mail);
                    client.Disconnect(true);
                };
            }
            catch(Exception ex)
            {
                throw new CannotSendEmailException("Cannot send email" + ex.Message, ex);
            }
        }
    }
}
