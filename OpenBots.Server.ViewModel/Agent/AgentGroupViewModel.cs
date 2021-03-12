using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AgentGroupViewModel : NamedEntity, IViewModel<AgentGroup, AgentGroupViewModel>
    {
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public IEnumerable<AgentGroupMember>? AgentGroupMembers { get; set; }

        public AgentGroupViewModel Map(AgentGroup entity)
        {
            AgentGroupViewModel automationViewModel = new AgentGroupViewModel();

            automationViewModel.Id = entity.Id;
            automationViewModel.Name = entity.Name;
            automationViewModel.IsEnabled = entity.IsEnabled;
            automationViewModel.Description = entity.Description;
            automationViewModel.CreatedBy = entity.CreatedBy;
            automationViewModel.CreatedOn = entity.CreatedOn;
            automationViewModel.DeletedBy = entity.DeletedBy;
            automationViewModel.DeleteOn = entity.DeleteOn;
            automationViewModel.IsDeleted = entity.IsDeleted;
            automationViewModel.Timestamp = entity.Timestamp;
            automationViewModel.UpdatedBy = entity.UpdatedBy;
            automationViewModel.UpdatedOn = entity.UpdatedOn;

            return automationViewModel;
        }
    }
}
