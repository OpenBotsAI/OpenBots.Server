using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Webhooks
{
    public class IntegrationEventSubscription : NamedEntity
    {
        public string? EntityType { get; set; }

        public string? IntegrationEventName { get; set; }

        public Guid? EntityID { get; set; }

        public string? EntityName { get; set; }

        [Required]
        public TransportType? TransportType { get; set; }

        public string? HTTP_URL { get; set; }

        public string? HTTP_AddHeader_Key  { get; set; }

        public string? HTTP_AddHeader_Value  { get; set; }

        public int? HTTP_Max_RetryCount { get; set; }

        public Guid? QUEUE_QueueID { get; set; }
    }

    public enum TransportType : int
    {
        HTTPS = 1,
        Queue = 2,
        SignalR = 3
    }

}
