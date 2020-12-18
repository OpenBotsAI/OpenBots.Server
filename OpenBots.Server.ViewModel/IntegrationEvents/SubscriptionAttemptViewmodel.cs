using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class SubscriptionAttemptViewmodel : IViewModel<IntegrationEventSubscriptionAttempt, SubscriptionAttemptViewmodel>
    {
        public Guid? Id { get; set; }
        public string? TransportType { get; set; }
        public Guid? EventLogID { get; set; }
        public Guid? IntegrationEventSubscriptionID { get; set; }
        public string? IntegrationEventName { get; set; }
        public string? Status { get; set; }
        public int? AttemptCounter { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }

        public SubscriptionAttemptViewmodel Map(IntegrationEventSubscriptionAttempt entity)
        {
            SubscriptionAttemptViewmodel attemptViewModel = new SubscriptionAttemptViewmodel
            {
                Id = entity.Id,
                EventLogID = entity.EventLogID,
                IntegrationEventSubscriptionID = entity.IntegrationEventSubscriptionID,
                IntegrationEventName = entity.IntegrationEventName,
                Status = entity.Status,
                AttemptCounter = entity.AttemptCounter,
                CreatedOn = entity.CreatedOn,
                CreatedBy = entity.CreatedBy
            };

            return attemptViewModel;
        }
    }
}
