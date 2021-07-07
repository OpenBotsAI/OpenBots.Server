using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business
{
    public interface IIntegrationEventManager : IManager
    {
        IntegrationEvent AddBusinessEvent(CreateBusinessEventViewModel createBusinessEventView);
        IntegrationEvent UpdateBusinessEvent(string id, CreateBusinessEventViewModel request);
        void AttemptPatchUpdate(JsonPatchDocument<IntegrationEvent> request, string id);
        void RaiseBusinessEvent(string id, RaiseBusinessEventViewModel request);
        void DeleteBusinessEvent(string id);
    }
}
