using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.WebHooks
{
    public class IntegrationEventSubscriptionAttempt : Entity, INonAuditable
    {
        public string? EventLogID { get; set; }
        public Guid? IntegrationEventSubscription { get; set; }
        public string? Status { get; set; }
        public int? AttemptCounter { get; set; }
    }
}
