using Microsoft.AspNetCore.Http;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
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
        private readonly IFileManager _fileManager;
        private readonly IAutomationVersionRepository _automationVersionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutomationManager(
            IAutomationRepository repo,
            IFileManager fileManager,
            IAutomationVersionRepository automationVersionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _fileManager = fileManager;
            _automationVersionRepository = automationVersionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public Automation AddAutomation(AutomationViewModel request)
        {
            if (request.DriveName != "Files" && !string.IsNullOrEmpty(request.DriveName))
                throw new EntityOperationException("Component files can only be saved in the Files drive");
            else if (string.IsNullOrEmpty(request.DriveName))
                request.DriveName = "Files";

            IFormFile[] fileArray = { request.File };
            string path = Path.Combine(request.DriveName, "Automations", request.Id.ToString());

            var fileView = new FileFolderViewModel()
            {
                Files = fileArray,
                StoragePath = path,
                FullStoragePath = path,
                ContentType = fileArray[0].ContentType,
                IsFile = true
            };

            CheckStoragePathExists(fileView, request);
            fileView = _fileManager.AddFileFolder(fileView, request.DriveName)[0];

            var automation = new Automation()
            {
                Name = request.Name,
                AutomationEngine = request.AutomationEngine,
                Id = request.Id,
                FileId = fileView.Id,
                OriginalPackageName = request.File.FileName
            };

            AddAutomationVersion(request);

            return automation;
        }

        public Automation UpdateAutomationFile(string id, AutomationViewModel request)
        {
             Guid entityId = new Guid(id);
             var existingAutomation = _repo.GetOne(entityId);
             if (existingAutomation == null) throw new EntityDoesNotExistException($"Automation with id {id} could not be found");

             string fileId = existingAutomation.FileId.ToString();
             var file = _fileManager.GetFileFolder(fileId, request.DriveName);
             if (file == null) throw new EntityDoesNotExistException($"Automation file with id {fileId} could not be found");

             var response = AddAutomation(request);
             return response;
        }

        public FileFolderViewModel CheckStoragePathExists(FileFolderViewModel view, AutomationViewModel request)
        {
            //check if storage path exists; if it doesn't exist, create folder
            var folder = _fileManager.GetFileFolderByStoragePath(view.FullStoragePath, request.DriveName);
            if (folder.Name == null)
            {
                folder.Name = request.Id.ToString();
                folder.StoragePath = Path.Combine(request.DriveName, "Automations");
                folder.IsFile = false;
                folder.Size = request.File.Length;
                folder = _fileManager.AddFileFolder(folder, request.DriveName)[0];
            }
            return folder;
        }

        public Automation UpdateAutomation(string id, AutomationViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingAutomation = _repo.GetOne(entityId);
            if (existingAutomation == null) throw new EntityDoesNotExistException($"Automation with id {id} could not be found");

            existingAutomation.Name = request.Name;
            existingAutomation.AutomationEngine = request.AutomationEngine;

            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == existingAutomation.Id).Items?.FirstOrDefault();
            if (!string.IsNullOrEmpty(automationVersion.Status))
            {
                //check if previous value was not published before setting published properties
                automationVersion.Status = request.Status;
                if (automationVersion.Status == "Published")
                {
                    automationVersion.PublishedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                    automationVersion.PublishedOnUTC = DateTime.UtcNow;
                }
                _automationVersionRepository.Update(automationVersion);
            }
            return existingAutomation;
        }

        public async Task<FileFolderViewModel> Export(string id, string driveName)
        {
            Guid automationId;
            Guid.TryParse(id, out automationId);

            Automation automation = _repo.GetOne(automationId);

            if (automation == null || automation.FileId == null || automation.FileId == Guid.Empty)
                throw new EntityDoesNotExistException($"Automation with id {id} could not be found");

            var response = await _fileManager.ExportFileFolder(automation.FileId.ToString(), driveName);
            return response;
        }

        public void DeleteAutomation(Automation automation, string driveName)
        {
            //remove file associated with automation
            _fileManager.DeleteFileFolder(automation.FileId.ToString(), driveName);
            _repo.SoftDelete(automation.Id.Value);

            //remove automation version entity associated with automation
            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == automation.Id).Items?.FirstOrDefault();
            _automationVersionRepository.SoftDelete(automationVersion.Id.Value);
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