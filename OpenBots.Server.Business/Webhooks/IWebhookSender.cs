using OpenBots.Server.Model.Webhooks;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Webhooks
{
    public interface IWebhookSender
    {
        Task SendWebhook(IntegrationEventSubscription eventSubscription, WebhookPayload payload,
        IntegrationEventSubscriptionAttempt subscriptionAttempt);
    }
}
