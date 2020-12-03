using OpenBots.Server.Model.Core;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IEmailManager
    {
        Task SendEmailAsync(EmailMessage emailMessage, string accountName = null, string id = null, string direction = null);
        bool IsEmailAllowed();
    }
}