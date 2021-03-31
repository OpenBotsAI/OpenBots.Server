using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business
{
    public interface IIntegrationEventManager : IManager
    {
        IntegrationEvent UpdateBusinessEvent(string id, CreateBusinessEventViewModel request);
        void RaiseBusinessEvent(string id, RaiseBusinessEventViewModel request);
    }
}
