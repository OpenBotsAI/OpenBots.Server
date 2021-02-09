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
        private readonly IAutomationRepository _repo;
        private readonly IBinaryObjectRepository _binaryObjectRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IBlobStorageAdapter _blobStorageAdapter;
        private readonly IAutomationVersionRepository _automationVersionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutomationManager(
            IAutomationRepository repo,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IBlobStorageAdapter blobStorageAdapter,
            IAutomationVersionRepository automationVersionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _binaryObjectManager = binaryObjectManager;
            _binaryObjectRepository = binaryObjectRepository;
            _blobStorageAdapter = blobStorageAdapter;
            _automationVersionRepository = automationVersionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FileObjectViewModel> Export(string binaryObjectId)
        {
            return await _blobStorageAdapter.FetchFile(binaryObjectId);
        }

        public bool DeleteAutomation(Guid automationId)
        {
            var automation = _repo.GetOne(automationId);

            //remove automation version entity associated with automation
            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == automationId).Items?.FirstOrDefault();
            Guid automationVersionId = (Guid)automationVersion.Id;
            _automationVersionRepository.SoftDelete(automationVersionId);
            
            bool isDeleted = false;

            if (automation != null)
            {
                //remove binary object entity associated with automation
                _binaryObjectRepository.SoftDelete(automation.BinaryObjectId);
                _repo.SoftDelete(automation.Id.Value);

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
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                BinaryObjectId = (Guid)request.BinaryObjectId,
                OriginalPackageName = request.File.FileName,
                AutomationEngine = request.AutomationEngine
            };

            _repo.Add(automation);
            AddAutomationVersion(request);

            return automation;
        }

        public async Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "")
        {
            //update file in OpenBots.Server.Web using relative directory
            _binaryObjectManager.Update(file, organizationId, apiComponent, binaryObjectId);

            //find relative directory where binary object is being saved
            string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObjectId.ToString());

            await _binaryObjectManager.UpdateEntity(file, filePath, binaryObjectId.ToString(), apiComponent, apiComponent, name);

            return "Success";
        }

        public string GetOrganizationId()
        {
            return _binaryObjectManager.GetOrganizationId();
        }

        public void AddAutomationVersion(AutomationViewModel automationViewModel)
        {
            AutomationVersion automationVersion = new AutomationVersion();
            automationVersion.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
            automationVersion.CreatedOn = DateTime.UtcNow;
            automationVersion.AutomationId = (Guid)automationViewModel.Id;

            if (string.IsNullOrEmpty(automationViewModel.Status))
                automationVersion.Status = "Published";
            else automationVersion.Status = automationViewModel.Status;
            automationVersion.VersionNumber = automationViewModel.VersionNumber;

            if (automationVersion.Status.Equals("Published"))
            {
                automationVersion.PublishedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                automationVersion.PublishedOnUTC = DateTime.UtcNow;
            }
            else
            {
                automationVersion.PublishedBy = null;
                automationVersion.PublishedOnUTC = null;
            }

            int automationVersionNumber = 0;
            automationVersion.VersionNumber = automationVersionNumber;
            List<Automation> automations = _repo.Find(null, x => x.Name?.Trim().ToLower() == automationViewModel.Name?.Trim().ToLower())?.Items;

            if (automations != null)
                foreach (Automation automation in automations)
                {
                    var automationVersionEntity = _automationVersionRepository.Find(null, q => q?.AutomationId == automation?.Id).Items?.FirstOrDefault();
                    if (automationVersionEntity != null && automationVersionNumber < automationVersionEntity.VersionNumber)
                    {
                        automationVersionNumber = automationVersionEntity.VersionNumber;
                    }
                }

            automationVersion.VersionNumber = automationVersionNumber + 1;

            _automationVersionRepository.Add(automationVersion);
        }

        public PaginatedList<AllAutomationsViewModel> GetAutomationsAndAutomationVersions(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public AutomationViewModel GetAutomationView(AutomationViewModel automationView, string id)
        {
            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == Guid.Parse(id))?.Items?.FirstOrDefault();
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