using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business
{
    public interface IIntegrationEventSubscriptionManager : IManager
    {
        IntegrationEventSubscription AddIntegrationEventSubscription(IntegrationEventSubscription schedule);
        IntegrationEventSubscription UpdateIntegrationEventSubscription(string id, IntegrationEventSubscription request);
        void AttemptPatchUpdate(JsonPatchDocument<IntegrationEventSubscription> request, string id);
    }
}
