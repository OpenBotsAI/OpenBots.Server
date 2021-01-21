using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Webhooks
{
    public interface IWebhookSender
    {
        Task SendWebhook(IntegrationEventSubscription eventSubscription, WebhookPayload payload,
        IntegrationEventSubscriptionAttempt subscriptionAttempt);

    }
}
