using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class AllProcessesViewModel : IViewModel<Process, AllProcessesViewModel>
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }

        public AllProcessesViewModel Map(Process entity)
        {
            AllProcessesViewModel processViewModel = new AllProcessesViewModel();

            processViewModel.Id = entity.Id;
            processViewModel.Name = entity.Name;
            processViewModel.CreatedBy = entity.CreatedBy;
            processViewModel.CreatedOn = entity.CreatedOn;

            return processViewModel;
        }
    }
}
