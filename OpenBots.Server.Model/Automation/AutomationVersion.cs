using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model
{
    public class AutomationVersion : Entity
    {
        public Guid AutomationId { get; set; }
        public int VersionNumber { get; set; }
        public string? PublishedBy { get; set; }
        public DateTime? PublishedOnUTC { get; set; }
        public string Status { get; set; }
    }
}
