using System.Threading.Tasks;

namespace OpenBots.Server.Web.Webhooks
{
    public interface IWebhookPublisher
    {
        Task PublishAsync(string integrationEventName, string entityId = "", string entityName = "");
    }
}
