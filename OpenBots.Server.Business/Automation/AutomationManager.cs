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

        public AutomationManager(
            IAutomationRepository repo,
            IFileManager fileManager,
            IAutomationVersionRepository automationVersionRepository,
            IAutomationParameterRepository automationParameterRepository,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _repo = repo;
            _fileManager = fileManager;
            _automationVersionRepository = automationVersionRepository;
            _automationParameterRepository = automationParameterRepository;
            _httpContextAccessor = httpContextAccessor;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
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
            var file = _fileManager.GetFileFolder(automation.FileId.ToString(), driveName);
            _fileManager.DeleteFileFolder(automation.FileId.ToString(), driveName);
            var folder = _fileManager.GetFileFolder(file.ParentId.ToString(), driveName);
            _fileManager.DeleteFileFolder(folder.Id.ToString(), driveName);
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
                automationView.AutomtationParameters = GetAutomationParameters(automationView.Id);
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
                _automationParameterRepository.SoftDelete(parmeter.Id ?? Guid.Empty);
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
    }
}