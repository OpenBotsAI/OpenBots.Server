using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class AllAutomationsViewModel : IViewModel<Automation, AllAutomationsViewModel>
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public int VersionNumber { get; set; }

        public AllAutomationsViewModel Map(Automation entity)
        {
            AllAutomationsViewModel automationViewModel = new AllAutomationsViewModel();

            automationViewModel.Id = entity.Id;
            automationViewModel.Name = entity.Name;
            automationViewModel.CreatedBy = entity.CreatedBy;
            automationViewModel.CreatedOn = entity.CreatedOn;

            return automationViewModel;
        }
    }
}
