using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class AutomationViewModel : NamedEntity, IViewModel<Automation, AutomationViewModel>
    {
        public int VersionNumber { get; set; }
        public Guid? VersionId { get; set; }
        public string? Status { get; set; }
        public IFormFile? File { get; set; }
        public Guid? BinaryObjectId { get; set; }
        public string? OriginalPackageName { get; set; }
        public string? PublishedBy { get; set; }
        public DateTime? PublishedOnUTC { get; set; }
        public string AutomationEngine { get; set; }

        public AutomationViewModel Map(Automation entity)
        {
            AutomationViewModel automationViewModel = new AutomationViewModel();

            automationViewModel.Id = entity.Id;
            automationViewModel.BinaryObjectId = entity.BinaryObjectId;
            automationViewModel.CreatedBy = entity.CreatedBy;
            automationViewModel.CreatedOn = entity.CreatedOn;
            automationViewModel.DeletedBy = entity.DeletedBy;
            automationViewModel.DeleteOn = entity.DeleteOn;
            automationViewModel.IsDeleted = entity.IsDeleted;
            automationViewModel.OriginalPackageName = entity.OriginalPackageName;
            automationViewModel.Timestamp = entity.Timestamp;
            automationViewModel.UpdatedBy = entity.UpdatedBy;
            automationViewModel.UpdatedOn = entity.UpdatedOn;
            automationViewModel.Name = entity.Name;
            automationViewModel.AutomationEngine = entity.AutomationEngine;

            return automationViewModel;
        }
    }
}
