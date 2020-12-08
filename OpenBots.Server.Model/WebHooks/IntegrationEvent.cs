using OpenBots.Server.Model.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Webhooks
{
    public class IntegrationEvent : NamedEntity
    {
        [StringLength(2048, ErrorMessage = "The Description cannot exceed 2048 characters. ")]
        public string Description { get; set; }

        [StringLength(256, ErrorMessage = "The EntityName cannot exceed 256 characters. ")]
        public string? EntityType { get; set; }

        public string? PayloadSchema { get; set; }

        [DefaultValue(true)]
        public bool IsSystem { get; set; }
    }
}
