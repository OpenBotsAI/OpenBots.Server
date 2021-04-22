using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model
{
    public class AgentGroupMember : Entity
    {
        [Display(Name = "AgentGroupId")]
        public Guid AgentGroupId { get; set; }

        [Required]
        [Display(Name = "AgentId")]
        public Guid? AgentId { get; set; }
    }
}
