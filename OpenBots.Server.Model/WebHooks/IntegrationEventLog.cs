using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.WebHooks
{
    public class IntegrationEventLog : NamedEntity, INonAuditable
    {
        public DateTime? OccuredOnUTC { get; set; }

        public string? EntityName { get; set; }

        public Guid? EntityID { get; set; }

        public string? PayloadJSON { get; set; }

        public string? Message { get; set; }

        public string? Status { get; set; }

        public string? SHA256Hash { get; set; }
    }
}
