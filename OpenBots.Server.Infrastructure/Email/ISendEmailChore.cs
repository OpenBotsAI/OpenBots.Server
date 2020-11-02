using OpenBots.Server.Model.Core;

namespace OpenBots.Server.Infrastructure.Email
{
    public interface ISendEmailChore
    {
        void SendEmail(EmailMessage message);
    }
}
