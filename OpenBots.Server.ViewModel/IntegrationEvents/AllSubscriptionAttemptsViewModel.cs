using OpenBots.Server.Model.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AllSubscriptionAttemptsViewModel
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
    }
}
