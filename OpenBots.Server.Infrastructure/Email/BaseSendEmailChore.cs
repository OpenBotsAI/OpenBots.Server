using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.Infrastructure.Email
{
    public abstract class BaseSendEmailChore : ISendEmailChore
    {
        protected EmailSettings setting;
        protected EmailAccount emailAccount;
        public BaseSendEmailChore(EmailSettings sendEmailSetting, EmailAccount emailAccount)
        {
            setting = sendEmailSetting;
            this.emailAccount = emailAccount;
        }

        public abstract void SendEmail(EmailMessage message);
    }
}
