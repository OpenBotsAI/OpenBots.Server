using OpenBots.Server.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class RunNowViewModel
    {
        public Guid AgentId { get; set; }
        [Required]
        public Guid AutomationId { get; set; }
        public IEnumerable<ParametersViewModel>? JobParameters { get; set; }
    }
}
