using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class RunNowViewModel
    {
        public Guid AgentId { get; set; }
        public Guid AgentGroupId { get; set; }
        [Required]
        public Guid AutomationId { get; set; }
        public IEnumerable<ParametersViewModel>? JobParameters { get; set; }
    }
}
