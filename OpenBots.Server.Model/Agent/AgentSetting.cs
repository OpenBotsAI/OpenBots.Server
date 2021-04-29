using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    public class AgentSetting : Entity
    {
        [Required]
        public Guid? AgentId { get; set; }

        [Range(30, int.MaxValue, ErrorMessage = "Please enter valid a number greater than or equal to 30")]
        public int? HeartbeatInterval { get; set; }

        [Range(5, int.MaxValue, ErrorMessage = "Please enter valid a number greater than or equal to 5")]
        public int? JobLoggingInterval { get; set; }

        public bool? VerifySslCertificate { get; set; }
    }
}
