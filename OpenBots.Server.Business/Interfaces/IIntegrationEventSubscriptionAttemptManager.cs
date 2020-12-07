using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business
{
    public interface IIntegrationEventSubscriptionAttemptManager : IManager
    {
        int? SaveAndGetAttemptCount(IntegrationEventSubscriptionAttempt subscriptionAttempt, int? maxRetryCount);
        IntegrationEventSubscriptionAttempt GetLastAttempt(IntegrationEventSubscriptionAttempt subscriptionAttempt);
    }
}
