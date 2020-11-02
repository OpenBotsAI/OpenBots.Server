using OpenBots.Server.Model.Core;

namespace OpenBots.Server.Infrastructure.Email
{
    public class NullSendEmailChore : ISendEmailChore
    {
        public void SendEmail(EmailMessage message)
        {
        }
    }
}
