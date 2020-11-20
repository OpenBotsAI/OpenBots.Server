using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model
{
    public class ProcessVersion : Entity
    {
        public Guid ProcessId { get; set; }
        public int VersionNumber { get; set; }
        public string? PublishedBy { get; set; }
        public DateTime? PublishedOnUTC { get; set; }
        public string Status { get; set; }
    }
}
