using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class ProcessViewModel : NamedEntity, IViewModel<Process, ProcessViewModel>
    {
        public int VersionNumber { get; set; }
        public Guid? VersionId { get; set; }
        public string? Status { get; set; }
        public IFormFile? File { get; set; }
        public Guid? BinaryObjectId { get; set; }
        public string? OriginalPackageName { get; set; }
        public string? PublishedBy { get; set; }
        public DateTime? PublishedOnUTC { get; set; }

        public ProcessViewModel Map(Process entity)
        {
            ProcessViewModel processViewModel = new ProcessViewModel();

            processViewModel.Id = entity.Id;
            processViewModel.BinaryObjectId = entity.BinaryObjectId;
            processViewModel.CreatedBy = entity.CreatedBy;
            processViewModel.CreatedOn = entity.CreatedOn;
            processViewModel.DeletedBy = entity.DeletedBy;
            processViewModel.DeleteOn = entity.DeleteOn;
            processViewModel.IsDeleted = entity.IsDeleted;
            processViewModel.OriginalPackageName = entity.OriginalPackageName;
            processViewModel.Timestamp = entity.Timestamp;
            processViewModel.UpdatedBy = entity.UpdatedBy;
            processViewModel.UpdatedOn = entity.UpdatedOn;
            processViewModel.Name = entity.Name;

            return processViewModel;
        }
    }
}
