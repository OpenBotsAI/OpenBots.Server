using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model.Identity
{
    [NotMapped]
    public class VerifyAgentModel
    {
        [Required]
        public string AgentId { get; set; }
        [Required]
        public string MachineName { get; set; }
        [Required]
        public string MacAddresses { get; set; }
    }
}
