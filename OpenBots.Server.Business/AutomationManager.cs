using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class AutomationManager : BaseManager, IAutomationManager
    {
        private readonly IAutomationRepository repo;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBlobStorageAdapter blobStorageAdapter;
        private readonly IAutomationVersionRepository automationVersionRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AutomationManager(IAutomationRepository repo,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IBlobStorageAdapter blobStorageAdapter,
            IAutomationVersionRepository automationVersionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            this.repo = repo;
            this.binaryObjectManager = binaryObjectManager;
            this.binaryObjectRepository = binaryObjectRepository;
            this.blobStorageAdapter = blobStorageAdapter;
            this.automationVersionRepository = automationVersionRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<FileObjectViewModel> Export(string binaryObjectId)
        {
            return await blobStorageAdapter.FetchFile(binaryObjectId);
        }

        public bool DeleteAutomation(Guid automationId)
        {
            var automation = repo.GetOne(automationId);

            // Remove Automation version entity associated with Automation
            var automationVersion = automationVersionRepository.Find(null, q => q.AutomationId == automationId).Items?.FirstOrDefault();
            Guid automationVersionId = (Guid)automationVersion.Id;
            automationVersionRepository.SoftDelete(automationVersionId);
            
            bool isDeleted = false;

            if (automation != null)
            {
                // Remove binary object entity associated with automation
                binaryObjectRepository.SoftDelete(automation.BinaryObjectId);
                repo.SoftDelete(automation.Id.Value);

                isDeleted = true;
            }

            return isDeleted;
        }

        public Automation UpdateAutomation(Automation existingAutomation, AutomationViewModel request)
        {
            Automation automation = new Automation()
            {
                Id = request.Id,
                Name = request.Name,
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                BinaryObjectId = existingAutomation.BinaryObjectId,
                OriginalPackageName = existingAutomation.OriginalPackageName
            };

            request.Id = automation.Id;
            AddAutomationVersion(request);  

            return repo.Add(automation);
        }

        public async Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "")
        {
            //Update file in OpenBots.Server.Web using relative directory
            binaryObjectManager.Update(file, organizationId, apiComponent, binaryObjectId);

            //find relative directory where binary object is being saved
            string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObjectId.ToString());

            await binaryObjectManager.UpdateEntity(file, filePath, binaryObjectId.ToString(), apiComponent, apiComponent, name);

            return "Success";
        }

        public string GetOrganizationId()
        {
            return binaryObjectManager.GetOrganizationId();
        }

        public void AddAutomationVersion(AutomationViewModel automationViewModel)
        {
            AutomationVersion automationVersion = new AutomationVersion();
            automationVersion.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
            automationVersion.CreatedOn = DateTime.UtcNow;
            automationVersion.AutomationId = (Guid)automationViewModel.Id;
            if (string.IsNullOrEmpty(automationViewModel.Status))
                automationVersion.Status = "Published";
            else automationVersion.Status = automationViewModel.Status;
            automationVersion.VersionNumber = automationViewModel.VersionNumber;
            if (automationVersion.Status.Equals("Published"))
            {
                automationVersion.PublishedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                automationVersion.PublishedOnUTC = DateTime.UtcNow;
            }
            else
            {
                automationVersion.PublishedBy = null;
                automationVersion.PublishedOnUTC = null;
            }

            int automationVersionNumber = 0;
            automationVersion.VersionNumber = automationVersionNumber;
            List<Automation> automationes = repo.Find(null, x => x.Name?.Trim().ToLower() == automationViewModel.Name?.Trim().ToLower())?.Items;

            if (automationes != null)
                foreach (Automation automation in automationes)
                {
                    var automationVersionEntity = automationVersionRepository.Find(null, q => q?.AutomationId == automation?.Id).Items?.FirstOrDefault();
                    if (automationVersionEntity != null && automationVersionNumber < automationVersionEntity.VersionNumber)
                    {
                        automationVersionNumber = automationVersionEntity.VersionNumber;
                    }
                }

            automationVersion.VersionNumber = automationVersionNumber + 1;

            automationVersionRepository.Add(automationVersion);
        }

        public PaginatedList<AllAutomationsViewModel> GetAutomationsAndAutomationVersions(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public AutomationViewModel GetAutomationView(AutomationViewModel automationView, string id)
        {
            var automationVersion = automationVersionRepository.Find(null, q => q.AutomationId == Guid.Parse(id))?.Items?.FirstOrDefault();
            if (automationVersion != null)
            {
                automationView.VersionId = (Guid)automationVersion.Id;
                automationView.VersionNumber = automationVersion.VersionNumber;
                automationView.Status = automationVersion.Status;
                automationView.PublishedBy = automationVersion.PublishedBy;
                automationView.PublishedOnUTC = automationVersion.PublishedOnUTC;
            }

            return automationView;
        }
    }
}