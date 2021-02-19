using OpenBots.Server.Model.Core;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    public class AgentGroup : NamedEntity
    {
        [Required]
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
    }
}