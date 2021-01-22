using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using System;

namespace OpenBots.Server.ViewModel
{
    public class SubscriptionAttemptViewModel : IViewModel<IntegrationEventSubscriptionAttempt, SubscriptionAttemptViewModel>
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

        public SubscriptionAttemptViewModel Map(IntegrationEventSubscriptionAttempt entity)
        {
            SubscriptionAttemptViewModel attemptViewModel = new SubscriptionAttemptViewModel
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
