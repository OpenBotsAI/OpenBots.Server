using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class ProcessExecutionViewModel : IViewModel<ProcessExecutionLog, ProcessExecutionViewModel>
    {
        public Guid? Id { get; set; }
        public string AgentName { get; set; }
        public string ProcessName { get; set; }
        public Guid? JobID { get; set; }
        public Guid ProcessID { get; set; }
        public Guid AgentID { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string Trigger { get; set; }
        public string TriggerDetails { get; set; }
        public string? Status { get; set; }
        public bool? HasErrors { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }

        public ProcessExecutionViewModel Map(ProcessExecutionLog entity)
        {
            ProcessExecutionViewModel processExecutionViewModel = new ProcessExecutionViewModel
            {
                Id = entity.Id,
                JobID = entity.JobID,
                ProcessID = entity.ProcessID,
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

            return processExecutionViewModel;
        }
    }
}
