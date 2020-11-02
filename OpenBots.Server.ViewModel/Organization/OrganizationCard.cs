using System;

namespace OpenBots.Server.ViewModel
{
    public class OrganizationCard
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? JoinedOn { get; set; }

        public bool CanLeave { get; set; }

        public bool CanDelete { get; set; }

        public bool IsOrganizationMember { get; set; }
    }
}
