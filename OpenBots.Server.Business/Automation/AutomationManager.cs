using Microsoft.AspNetCore.Http;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class AutomationManager : BaseManager, IAutomationManager
    {
        private readonly IAutomationRepository _repo;
        private readonly IFileManager _fileManager;
        private readonly IAutomationVersionRepository _automationVersionRepository;
        private readonly IAutomationParameterRepository _automationParameterRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClaimsPrincipal _caller;
        private readonly IStorageDriveRepository _storageDriveRepository;
        private readonly IStorageFileRepository _storageFileRepository;

        public AutomationManager(
            IAutomationRepository repo,
            IFileManager fileManager,
            IAutomationVersionRepository automationVersionRepository,
            IAutomationParameterRepository automationParameterRepository,
            IHttpContextAccessor httpContextAccessor,
            IStorageDriveRepository storageDriveRepository,
            IStorageFileRepository storageFileRepository
            )
        {
            _repo = repo;
            _fileManager = fileManager;
            _automationVersionRepository = automationVersionRepository;
            _automationParameterRepository = automationParameterRepository;
            _httpContextAccessor = httpContextAccessor;
            _caller = (httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal();
            _storageDriveRepository = storageDriveRepository;
            _storageFileRepository = storageFileRepository;
        }

        public Automation AddAutomation(AutomationViewModel request)
        {
            var drive = new StorageDrive();
            if (string.IsNullOrEmpty(request.DriveId))
            {
                drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();
                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive does not exist or could not be found");
                else
                    request.DriveId = drive.Id.ToString();
            }
            else drive = _storageDriveRepository.GetOne(Guid.Parse(request.DriveId));

            IFormFile[] fileArray = { request.File };
            string shortPath = Path.Combine(drive.Name, "Automations");
            string path = Path.Combine(shortPath, request.Id.ToString());

            var fileView = new FileFolderViewModel()
            {
                Files = fileArray,
                StoragePath = shortPath,
                FullStoragePath = path,
                ContentType = fileArray[0].ContentType,
                IsFile = true
            };

            CheckStoragePathExists(fileView, request, drive.Name);
            fileView = _fileManager.AddFileFolder(fileView, request.DriveId)[0];

            var automationEngine = GetAutomationEngine(request.AutomationEngine);

            var automation = new Automation()
            {
                Name = request.Name,
                AutomationEngine = automationEngine,
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
            if (string.IsNullOrEmpty(request.DriveId))
            {
                var fileToUpdate = _storageFileRepository.GetOne(existingAutomation.FileId.Value);
                request.DriveId = fileToUpdate.StorageDriveId.ToString();
            }

            var file = _fileManager.GetFileFolder(fileId, request.DriveId, "Files");
            if (file == null) throw new EntityDoesNotExistException($"Automation file with id {fileId} could not be found");

            var response = AddAutomation(request);
            return response;
        }

        public void CheckStoragePathExists(FileFolderViewModel view, AutomationViewModel request, string driveName)
        {
            //check if storage path exists; if it doesn't exist, create folder
            var folder = _fileManager.GetFileFolderByStoragePath(view.StoragePath, driveName);
            CreateFolderIfEmpty(folder, request, driveName);
            folder = _fileManager.GetFileFolderByStoragePath(view.FullStoragePath, driveName);
            CreateFolderIfEmpty(folder, request, driveName);
        }

        public Automation UpdateAutomation(string id, AutomationViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingAutomation = _repo.GetOne(entityId);
            if (existingAutomation == null) throw new EntityDoesNotExistException($"Automation with id {id} could not be found");

            existingAutomation.Name = request.Name;
            existingAutomation.AutomationEngine = GetAutomationEngine(request.AutomationEngine);

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

        public async Task<FileFolderViewModel> Export(string id, string driveId)
        {
            Guid automationId;
            Guid.TryParse(id, out automationId);

            Automation automation = _repo.GetOne(automationId);

            if (automation == null || automation.FileId == null || automation.FileId == Guid.Empty)
                throw new EntityDoesNotExistException($"Automation with id {id} could not be found");

            driveId = CheckDriveIdByFileId(automation.FileId.ToString(), driveId);

            var response = await _fileManager.ExportFileFolder(automation.FileId.ToString(), driveId);
            return response;
        }

        public void DeleteAutomation(Automation automation)
        {
            var fileToDelete = _storageFileRepository.GetOne(automation.FileId.Value);
            string driveId = fileToDelete.StorageDriveId.ToString();

            //remove file associated with automation
            var file = _fileManager.GetFileFolder(automation.FileId.ToString(), driveId, "Files");
            _fileManager.DeleteFileFolder(automation.FileId.ToString(), driveId, "Files");
            var folder = _fileManager.GetFileFolder(file.ParentId.ToString(), driveId, "Folders");
            if (!folder.HasChild.Value)
                _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
            else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { file });

            //remove automation entity
            _repo.SoftDelete(automation.Id.Value);

            //remove automation version entity associated with automation
            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == automation.Id).Items?.FirstOrDefault();
            _automationVersionRepository.SoftDelete(automationVersion.Id.Value);
            DeleteExistingParameters(automation.Id);
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

        public AutomationViewModel GetAutomationView(AutomationViewModel automationView)
        {
            var automationVersion = _automationVersionRepository.Find(null, q => q.AutomationId == automationView.Id)?.Items?.FirstOrDefault();
            if (automationVersion != null)
            {
                automationView.VersionId = (Guid)automationVersion.Id;
                automationView.VersionNumber = automationVersion.VersionNumber;
                automationView.Status = automationVersion.Status;
                automationView.PublishedBy = automationVersion.PublishedBy;
                automationView.PublishedOnUTC = automationVersion.PublishedOnUTC;
                automationView.AutomationParameters = GetAutomationParameters(automationView.Id);
            }

            return automationView;
        }

        public IEnumerable<AutomationParameter> UpdateAutomationParameters(IEnumerable<AutomationParameter> automationParameters, string automationId)
        {
            Guid? entityId = Guid.Parse(automationId);

            CheckParameterNameAvailability(automationParameters);
            DeleteExistingParameters(entityId);
            return AddAutomationParameters(automationParameters, entityId);
        }

        public IEnumerable<AutomationParameter> AddAutomationParameters(IEnumerable<AutomationParameter> automationParameters, Guid? automationId)
        {
            List<AutomationParameter> parameterList = new List<AutomationParameter>();

            foreach (var parameter in automationParameters ?? Enumerable.Empty<AutomationParameter>())
            {
                parameter.AutomationId = automationId ?? Guid.Empty;
                parameter.CreatedBy = _caller.Identity.Name;
                parameter.CreatedOn = DateTime.UtcNow;
                parameter.Id = Guid.NewGuid();

                _automationParameterRepository.Add(parameter);
                parameterList.Add(parameter);
            }

            return parameterList.AsEnumerable();
        }

        public IEnumerable<AutomationParameter> GetAutomationParameters(Guid? automationId)
        {
            var automationParameters = _automationParameterRepository.Find(0, 1)?.Items?.Where(p => p.AutomationId == automationId);
            return automationParameters;
        }

        public void DeleteExistingParameters(Guid? automationId)
        {
            var automationParameters = GetAutomationParameters(automationId);
            foreach (var parmeter in automationParameters ?? Enumerable.Empty<AutomationParameter>())
            {
                _automationParameterRepository.Delete(parmeter.Id ?? Guid.Empty);
            }
        }

        public void CheckParameterNameAvailability(IEnumerable<AutomationParameter> automationParameters)
        {
            var set = new HashSet<string>();

            foreach (var parameter in automationParameters ?? Enumerable.Empty<AutomationParameter>())
            {
                if (!set.Add(parameter.Name))
                {
                    throw new Exception($"Automation parameter name \"{parameter.Name}\" already exists");
                }
            }
        }

        public string GetAutomationEngine(string requestEngine)
        {
            string automationEngine = null;
            if (requestEngine.ToLower() == AutomationEngines.CSScript.ToString().ToLower())
                automationEngine = AutomationEngines.CSScript.ToString();
            else if (requestEngine.ToLower() == AutomationEngines.OpenBots.ToString().ToLower())
                automationEngine = AutomationEngines.OpenBots.ToString();
            else if (requestEngine.ToLower() == AutomationEngines.Python.ToString().ToLower())
                automationEngine = AutomationEngines.Python.ToString();
            else if (requestEngine.ToLower() == AutomationEngines.TagUI.ToString().ToLower())
                automationEngine = AutomationEngines.TagUI.ToString();
            else
                throw new EntityOperationException($"Automation engine type {requestEngine} does not exist");

            return automationEngine;
        }
        
        public enum AutomationEngines
        {
            OpenBots,
            Python,
            CSScript,
            TagUI
        }

        private string CheckDriveId(string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();
                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
                else
                    driveId = drive.Id.ToString();
            }
            return driveId;
        }

        private string CheckDriveIdByFileId(string id, string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var fileToExport = _storageFileRepository.GetOne(Guid.Parse(id));
                driveId = fileToExport.StorageDriveId.ToString();
            }
            return driveId;
        }

        private void CreateFolderIfEmpty(FileFolderViewModel folder, AutomationViewModel request, string driveName)
        {
            if (string.IsNullOrEmpty(folder.Name))
            {
                folder.Name = request.Id.ToString();
                folder.StoragePath = Path.Combine(driveName, "Automations");
                folder.IsFile = false;
                folder.Size = request.File.Length;
                _fileManager.AddFileFolder(folder, request.DriveId);
            }
        }
    }
}