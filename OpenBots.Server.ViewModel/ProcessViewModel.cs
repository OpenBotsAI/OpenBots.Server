using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class ProcessViewModel : IViewModel<Process, ProcessViewModel>
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int Version { get; set; }
        public Guid VersionId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public IFormFile? File { get; set; }
        public Guid? BinaryObjectId { get; set; }
        public Guid? OrganizationId { get; set; }

        public ProcessViewModel Map(Process entity)
        {
            ProcessViewModel processViewModel = new ProcessViewModel();

            processViewModel.Id = entity.Id;
            processViewModel.Name = entity.Name;
            processViewModel.Version = entity.Version;
            processViewModel.VersionId = entity.VersionId;
            processViewModel.CreatedBy = entity.CreatedBy;
            processViewModel.CreatedOn = entity.CreatedOn;
            processViewModel.Status = entity.Status;

            return processViewModel;
        }
    }
}
