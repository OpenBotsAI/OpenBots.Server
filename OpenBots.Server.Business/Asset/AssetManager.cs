using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.File;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.ViewModel.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class AssetManager : BaseManager, IAssetManager
    {
        private readonly IAssetRepository _repo;
        private readonly IFileManager _fileManager;
        private readonly IPersonRepository _personRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IStorageFileRepository _storageFileRepository;
        private readonly IStorageDriveRepository _storageDriveRepository;

        public AssetManager(IAssetRepository assetRepository,
            IFileManager fileManager, 
            IPersonRepository personRepository,
            IAgentRepository agentRepository,
            IStorageFileRepository storageFileRepository,
            IStorageDriveRepository storageDriveRepository)
        {
            _repo = assetRepository;
            _fileManager = fileManager;
            _personRepository = personRepository;
            _agentRepository = agentRepository;
            _storageFileRepository = storageFileRepository;
            _storageDriveRepository = storageDriveRepository;
        }

        public Asset GetAsset(string id)
        {
            Guid entityId = new Guid(id);
            var asset = _repo.GetOne(entityId);
            if (asset == null) throw new EntityDoesNotExistException($"Asset with id {id} could not be found or doesn't exist");

            return asset;
        }

        public Asset CreateAsset(Asset asset, IFormFile file, string driveId = null)
        {
            AssetNameAvailability(asset);

            if (asset.Type == "Text")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.TextValue);
            }
            else if (asset.Type == "Number")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.NumberValue.ToString());
            }
            else if (asset.Type == "Json")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.JsonValue);
            }
            else if (asset.Type == "File")
            {
                var drive = new StorageDrive();
                if (string.IsNullOrEmpty(driveId))
                    drive = _storageDriveRepository.Find(null).Items.Where(q => q.IsDefault == true).FirstOrDefault();
                else drive = _fileManager.GetDriveById(driveId);

                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
                else   
                    driveId = drive.Id.ToString();

                if (file != null)
                {
                    IFormFile[] fileArray = { file };
                    asset.Id = Guid.NewGuid();
                    string shortPath = Path.Combine(drive.Name, "Assets", asset.Id.ToString());
                    var fileView = new FileFolderViewModel()
                    {
                        ContentType = file.ContentType,
                        Files = fileArray,
                        StoragePath = shortPath,
                        IsFile = true
                    };

                    var request = new AgentAssetViewModel();
                    request = request.Map(asset, file, drive.Id);
                    CheckStoragePathExists(fileView, request, false, drive);
                    fileView.StoragePath = Path.Combine(shortPath, asset.Id.ToString());
                    CheckStoragePathExists(fileView, request, true, drive);

                    fileView = _fileManager.AddFileFolder(fileView, driveId)[0];
                    asset.FileId = fileView.Id;
                    asset.SizeInBytes = file.Length;
                }
                else throw new EntityDoesNotExistException("File does not exist");
            }

            return asset;
        }

        public Asset CreateAgentAsset(AgentAssetViewModel request)
        {
            Asset globalAsset = _repo.Find(null, a => a.Name == request.Name && a.AgentId == null).Items?.FirstOrDefault();
            Asset agentAsset = new Asset();

            if (globalAsset == null)
            {
                throw new EntityDoesNotExistException("No global asset exists with the given name");
            }

            agentAsset.Name = request.Name;
            agentAsset.AgentId = request.AgentId;
            agentAsset.Type = globalAsset.Type;

            AssetNameAvailability(agentAsset);

            switch (agentAsset.Type.ToLower())
            {
                case "text":
                    if (request.TextValue == null)
                    {
                        agentAsset.TextValue = globalAsset.TextValue;
                    }
                    else
                    {
                        agentAsset.TextValue = request.TextValue;
                    }
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "number":
                    if (request.NumberValue == null)
                    {
                        agentAsset.NumberValue = globalAsset.NumberValue;
                    }
                    else
                    {
                        agentAsset.NumberValue = request.NumberValue;
                    }
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "json":
                    if (request.JsonValue == null)
                    {
                        agentAsset.JsonValue = globalAsset.JsonValue;
                    }
                    else
                    {
                        agentAsset.JsonValue = request.JsonValue;
                    }
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "file":
                    var drive = new StorageDrive();
                    string driveId = request.DriveId.ToString();
                    if (string.IsNullOrEmpty(driveId))
                        drive = _storageDriveRepository.Find(null).Items.Where(q => q.IsDefault == true).FirstOrDefault();
                    else drive = _fileManager.GetDriveById(driveId);

                    if (drive == null)
                        throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
                    else
                        driveId = drive.Id.ToString();

                    //if file is in request, use file; else, get file from global asset
                    IFormFile[] fileArray;
                    string agentIdStr = request.AgentId.ToString();
                    string storagePath = Path.Combine(drive.Name, "Assets", globalAsset.Id.ToString(), agentIdStr);

                    if (request.File != null)
                    {
                        fileArray = new IFormFile[] { request.File };
                        string fullStoragePath = Path.Combine(storagePath, request.File.FileName);
                        var fileView = new FileFolderViewModel()
                        {
                            ContentType = request.File.ContentType,
                            Files = fileArray,
                            StoragePath = storagePath,
                            IsFile = true,
                            FullStoragePath = fullStoragePath
                        };

                        CheckStoragePathExists(fileView, request, true, drive);

                        fileView = _fileManager.AddFileFolder(fileView, driveId)[0];
                        agentAsset.FileId = fileView.Id;
                        agentAsset.SizeInBytes = request.File.Length;
                    }
                    else
                    {
                        var fileViewModel = _fileManager.GetFileFolder(globalAsset.FileId.ToString(), driveId, "Files");
                        fileViewModel.StoragePath = storagePath;

                        var folder = CheckStoragePathExists(fileViewModel, request, true, drive);

                        fileViewModel = _fileManager.CopyFileFolder(fileViewModel.Id.ToString(), folder.Id.ToString(), driveId, "Files");
                        agentAsset.FileId = fileViewModel.Id;
                        agentAsset.SizeInBytes = fileViewModel.Size;
                    }
                    break;
            }

            return agentAsset;
        }

        public FileFolderViewModel CheckStoragePathExists(FileFolderViewModel view, AgentAssetViewModel request, bool getShortPath, StorageDrive drive)
        {
            //check if storage path exists; if it doesn't exist, create folder
            var folder = _fileManager.GetFileFolderByStoragePath(view.StoragePath, drive.Name);
            if (folder.Name == null)
            {
                string storagePath = view.StoragePath;
                if (getShortPath)
                    storagePath = _fileManager.GetShortPath(view.StoragePath);

                folder.Name = request.AgentId.ToString();
                folder.StoragePath = storagePath;
                folder.IsFile = false;
                folder.Size = (request.File == null) ? view.Size : request.File.Length;
                folder = _fileManager.AddFileFolder(folder, drive.Id.ToString())[0];
            }
            return folder;
        }

        public async Task<FileFolderViewModel> Export(string id, string driveId)
        {
            Guid assetId;
            Guid.TryParse(id, out assetId);

            Asset asset = _repo.GetOne(assetId);
            if (asset == null || asset.FileId == null || asset.FileId == Guid.Empty)
                throw new EntityDoesNotExistException($"Asset with id {id} could not be found or doesn't exist");

            driveId = CheckDriveIdByFileId(asset.FileId.ToString(), driveId);

            var response = await _fileManager.ExportFileFolder(asset.FileId.ToString(), driveId);
            return response;
        }

        public Asset GetSizeInBytes(Asset asset)
        {
            if (asset.Type == "Text")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.TextValue);
            }
            else if (asset.Type == "Number")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.NumberValue.ToString());
            }
            else if (asset.Type == "Json")
            {
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.JsonValue);
            }

            return asset;
        }

        public Asset UpdateAsset(string id, Asset request)
        {
            var existingAsset = GetAsset(id);
            
            existingAsset.TextValue = request.TextValue;
            existingAsset.NumberValue = request.NumberValue;
            existingAsset.JsonValue = request.JsonValue;
            existingAsset = GetSizeInBytes(existingAsset);

            AssetNameAvailability(existingAsset);

            return existingAsset;
        }

        public Asset UpdateAssetFile(string id, UpdateAssetViewModel request)
        {
            var existingAsset = GetAsset(id);

            string fileId = existingAsset.FileId.ToString();

            if (string.IsNullOrEmpty(request.DriveId))
            {
                var fileToUpdate = _storageFileRepository.GetOne(existingAsset.FileId.Value);
                request.DriveId = fileToUpdate.StorageDriveId.ToString();
            }

            var file = _fileManager.GetFileFolder(fileId, request.DriveId, "Files");
            if (file == null) throw new EntityDoesNotExistException($"Asset file with id {fileId} could not be found or doesn't exist");

            file.StoragePath = Path.Combine(file.StoragePath, request.File.FileName);
            file.ContentType = request.File.ContentType;
            file.Name = request.File.FileName;
            file.Size = request.File.Length;
            file.Files = new IFormFile[] { request.File };

            _fileManager.UpdateFile(file);

            existingAsset.SizeInBytes = file.Size;

            AssetNameAvailability(existingAsset);

            return existingAsset;
        }

        public Asset DeleteAsset(string id)
        {
            var asset = GetAsset(id);

            if (asset.AgentId == null)//asset is a global asset
            {
                var childAssets = _repo.Find(null, a => a.Name == asset.Name && a.AgentId != null)?.Items;

                if (childAssets.Count > 0)
                    throw new EntityOperationException("Child assets exist for this asset, please delete those first");
            }

            //remove file associated with asset
            if (asset.Type == "File")
            {
                var fileToDelete = _storageFileRepository.GetOne(asset.FileId.Value);
                string driveId = fileToDelete.StorageDriveId.ToString();
                var file = _fileManager.DeleteFileFolder(fileToDelete.Id.ToString(), driveId, "Files");
                var folder = _fileManager.GetFileFolder(file.ParentId.ToString(), driveId, "Folders");
                if (!folder.HasChild.Value)
                    _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
                else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { file });
            }

            return asset;
        }

        public Asset PatchAsset(string id, JsonPatchDocument<Asset> request)
        {
            var asset = GetAsset(id);

            for (int i = 0; i < request.Operations.Count; i++)
            {
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    asset.Name = request.Operations[i].value.ToString();
                    AssetNameAvailability(asset);
                }
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/agentid")
                {
                    //only update if asset is not global
                    if (asset.AgentId != null)
                    {
                        asset.AgentId = (Guid)request.Operations[i].value;
                    }
                }
            }

            return asset;
        }

        public Asset Increment(string id)
        {
            var asset = GetAsset(id);

            if (asset.Type != "Number") throw new EntityOperationException("Asset is not a number");

            asset.NumberValue = asset.NumberValue + 1;
            asset = GetSizeInBytes(asset);

            return asset;
        }

        public Asset Decrement(string id)
        {
            var asset = GetAsset(id);

            if (asset.Type != "Number") throw new EntityOperationException("Asset is not a number");

            asset.NumberValue = asset.NumberValue - 1;
            asset = GetSizeInBytes(asset);

            return asset;
        }

        public Asset Add(string id, int value)
        {
            var asset = GetAsset(id);

            if (asset.Type != "Number") throw new EntityOperationException("Asset is not a number");

            asset.NumberValue = asset.NumberValue + value;
            asset = GetSizeInBytes(asset);

            return asset;
        }

        public Asset Subtract(string id, int value)
        {
            var asset = GetAsset(id);

            if (asset.Type != "Number") throw new EntityOperationException("Asset is not a number");

            asset.NumberValue = asset.NumberValue - value;
            asset = GetSizeInBytes(asset);

            return asset;
        }

        public Asset Append(string id, string value)
        {
            var asset = GetAsset(id);

            if (asset.Type != "Text") throw new EntityOperationException("Asset is not text");

            asset.TextValue = string.Concat(asset.TextValue, " ", value);
            asset = GetSizeInBytes(asset);

            return asset;
        }

        public Asset GetMatchingAsset(string assetName, string assetType)
        {
            Guid? personId = SecurityContext.PersonId;
            Person callingPerson = _personRepository.Find(null, p => p.Id == personId)?.Items?.FirstOrDefault();
            List<Asset> assets;

            //if assetType was not specified
            if (string.IsNullOrEmpty(assetType))
            {
                assets = _repo.Find(null, a => a.Name == assetName)?.Items;
            }
            else
            {
                assets = _repo.Find(null, a => a.Name == assetName && a.Type.ToLower() == assetType.ToLower())?.Items;
            }

            if (assets.Count == 0)
            {
                throw new EntityDoesNotExistException("No asset was found that matches the provided details");
            }

            if (callingPerson.IsAgent)
            {
                Agent currentAgent = _agentRepository.Find(null, a => a.Name == callingPerson.Name)?.Items?.FirstOrDefault();
                var agentAsset = assets.Where(a => a.AgentId == currentAgent.Id)?.FirstOrDefault();

                if (agentAsset != null)
                {
                    return agentAsset;
                }
            }
            return assets.Where(a => a.AgentId == null).FirstOrDefault();
        }

        public void AssetNameAvailability(Asset request)
        {
            if (request.AgentId != null) //agent asset
            {
                var asset = _repo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null)
                    && d.AgentId == request.AgentId)?.Items?.FirstOrDefault();

                if (asset != null && asset.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("An asset with that name already exists for this agent");
                }
            }
            else //global asset
            {
                var asset = _repo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.AgentId == null)
                .Items?.FirstOrDefault();

                if (asset != null && asset.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("A global asset already exists with that name");
                }
            }
        }

        public AssetViewModel GetAssetDetails(AssetViewModel assetView)
        {
            assetView.AgentName = _agentRepository.Find(null, a => a.Id == assetView.AgentId).Items?.FirstOrDefault()?.Name;
            assetView.FileName = _storageFileRepository.Find(null, f => f.Id == assetView.FileId).Items?.FirstOrDefault()?.Name;

            return assetView;
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
    }
}
