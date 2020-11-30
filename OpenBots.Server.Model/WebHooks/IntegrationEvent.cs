using OpenBots.Server.Model.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.WebHooks
{
    public class IntegrationEvent : NamedEntity
    {
        [StringLength(2048, ErrorMessage = "The ThumbnailPhotoFileName value cannot exceed 4 characters. ")]
        public string Description { get; set; }

        [StringLength(256, ErrorMessage = "The ThumbnailPhotoFileName value cannot exceed 4 characters. ")]
        public string? EntityName { get; set; }

        public string? PayloadSchema { get; set; }

        [DefaultValue(true)]
        public bool IsSystem { get; set; }
    }
}
