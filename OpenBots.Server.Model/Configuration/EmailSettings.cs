using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.Configuration
{
    public class EmailSettings : Entity
    {
        public Guid OrganizationId { get; set; }
        public bool IsEmailDisabled { get; set; }
        public string AddToAddress { get; set; }
        public string AddCCAddress { get; set; }
        public string AddBCCAddress { get; set; }
        public string AllowedDomains { get; set; }
        public string BlockedDomains { get; set; }
        public string SubjectAddPrefix { get; set; }
        public string SubjectAddSuffix { get; set; }
        public string BodyAddPrefix { get; set; }
        public string BodyAddSuffix { get; set; }
    }
}
