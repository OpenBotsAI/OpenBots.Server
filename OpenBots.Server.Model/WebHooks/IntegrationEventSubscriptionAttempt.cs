using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Webhooks
{
    public class IntegrationEventSubscriptionAttempt : Entity, INonAuditable
    {
        [Required]
        public Guid? EventLogID { get; set; }

        [Required]
        public Guid? IntegrationEventSubscriptionID { get; set; }

        public string? IntegrationEventName { get; set; }

        public string? Status { get; set; }

        public int? AttemptCounter { get; set; }
    }
}
