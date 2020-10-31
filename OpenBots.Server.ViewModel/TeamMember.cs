﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class TeamMember
    {
        public Guid PersonId { get; set; }

        public Guid OrganizationMemberId { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Title { get; set; }

        public string EmailAddress { get; set; }

        public string Status { get; set; }

        public DateTime JoinedOn { get; set; }

        public string InvitedBy { get; set; }

        public bool? IsAdmin { get; set; }
    }
}
