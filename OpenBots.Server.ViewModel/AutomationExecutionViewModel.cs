using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class AutomationExecutionViewModel : IViewModel<AutomationExecutionLog, AutomationExecutionViewModel>
    {
        public Guid? Id { get; set; }
        public string AgentName { get; set; }
        public string AutomationName { get; set; }
        public Guid? JobID { get; set; }
        public Guid AutomationID { get; set; }
        public Guid AgentID { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string Trigger { get; set; }
        public string TriggerDetails { get; set; }
        public string? Status { get; set; }
        public bool? HasErrors { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }

        public AutomationExecutionViewModel Map(AutomationExecutionLog entity)
        {
            AutomationExecutionViewModel automationExecutionViewModel = new AutomationExecutionViewModel
            {
                Id = entity.Id,
                JobID = entity.JobID,
                AutomationID = entity.AutomationID,
                AgentID = entity.AgentID,
                StartedOn = entity.StartedOn,
                CompletedOn = entity.CompletedOn,
                Trigger = entity.Trigger,
                TriggerDetails = entity.TriggerDetails,
                Status = entity.Status,
                HasErrors = entity.HasErrors,
                ErrorMessage = entity.ErrorMessage,
                ErrorDetails = entity.ErrorDetails
            };

            return automationExecutionViewModel;
        }
    }
}
