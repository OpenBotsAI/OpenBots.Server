using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using System;
using System.Net.Mail;

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
                MailMessage mail = EmailMessage.FromStub(message);

                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(_smtpSetting.Username, _smtpSetting.EncryptedPassword);
                client.Port = _smtpSetting.Port; // You can use Port 25 if 587 is blocked
                client.Host = _smtpSetting.Host;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Send(mail);
            }
            catch(Exception ex)
            {
                throw new CannotSendEmailException("Cannot Send Email." + ex.Message, ex);
            }
        }
    }
}
