using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AgentGroupMemberViewModel
    {
        [Display(Name = "Id")]
        public Guid? Id { get; set; }

        [Display(Name = "AgentGroupId")]
        public Guid? AgentGroupId { get; set; }

        [Display(Name = "AgentId")]
        public Guid? AgentId { get; set; }

        [Display(Name = "AgentGroupName")]
        public string? AgentGroupName { get; set; }

        [Display(Name = "AgentName")]
        public string? AgentName { get; set; }
    }
}
