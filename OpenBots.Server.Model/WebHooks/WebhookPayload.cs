using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model.Webhooks
{
    [NotMapped]
    public class WebhookPayload
    {
        [Required]
        public Guid? EventId { get; set; }

        public string? EntityType { get; set; }

        public string? EventName { get; set; }

        public Guid? EntityID { get; set; }

        public string? EntityName { get; set; }

        public DateTime? OccuredOnUTC { get; set; }

        public string? Message { get; set; }

        public object? Data { get; set; }

        public string? HMACKey { get; set; }

        public string? NONCE { get; set; }

        public string? SHA256Hash { get; set; }

        public int? AttemptCount { get; set; }

    }
}

