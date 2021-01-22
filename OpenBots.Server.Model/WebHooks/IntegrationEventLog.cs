using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Webhooks
{
    public class IntegrationEventLog : Entity, INonAuditable
    {
        [Required]
        public string? IntegrationEventName { get; set; }

        [Required]
        public DateTime? OccuredOnUTC { get; set; }

        public string? EntityType { get; set; }

        public Guid? EntityID { get; set; }

        [Required]
        public string? PayloadJSON { get; set; }

        public string? Message { get; set; }

        public string? Status { get; set; }

        public string? SHA256Hash { get; set; }
    }
}
