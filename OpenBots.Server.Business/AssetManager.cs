using Microsoft.AspNetCore.Http;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.ViewModel.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace OpenBots.Server.Business
{
    public class AssetManager : BaseManager, IAssetManager
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ClaimsPrincipal _caller;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IBinaryObjectRepository _binaryObjectRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IAgentRepository _agentRepository;

        public AssetManager(IAssetRepository assetRepository,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IPersonRepository personRepository,
            IAgentRepository agentRepository)
        {
            _assetRepository = assetRepository;
            _binaryObjectManager = binaryObjectManager;
            _binaryObjectRepository = binaryObjectRepository;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
            _personRepository = personRepository;
            _agentRepository = agentRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _assetRepository.SetContext(userSecurityContext);
            _binaryObjectManager.SetContext(userSecurityContext);
            _binaryObjectRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Asset CreateAgentAsset(AgentAssetViewModel request)
        {
            Asset globalAsset = _assetRepository.Find(null, a => a.Name == request.Name && a.AgentId == null).Items?.FirstOrDefault();
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
                    agentAsset.TextValue = request.TextValue;
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "number":
                    if (request.NumberValue == null)
                    {
                        agentAsset.NumberValue = globalAsset.NumberValue;
                    }
                    agentAsset.NumberValue = request.NumberValue;
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "json":
                    if (request.JsonValue == null)
                    {
                        agentAsset.JsonValue = globalAsset.JsonValue;
                    }
                    agentAsset.JsonValue = request.JsonValue;
                    agentAsset = GetSizeInBytes(agentAsset);
                    break;
                case "file":
                    agentAsset = AddAssetFile(agentAsset, request.File);
                    break;
            }

            return agentAsset;
        }

        public Asset GetMatchingAsset(string assetName, string assetType)
        {
            Guid? personId = SecurityContext.PersonId;
            Person callingPerson = _personRepository.Find(null, p => p.Id == personId)?.Items?.FirstOrDefault();
            List<Asset> assets;

            //if assetType was not specified
            if (String.IsNullOrEmpty(assetType))
            {
                assets = _assetRepository.Find(null, a => a.Name == assetName)?.Items;
            }
            else
            {
                assets = _assetRepository.Find(null, a => a.Name == assetName && a.Type.ToLower() == assetType.ToLower())?.Items;
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
                var asset = _assetRepository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null)
                    && d.AgentId == request.AgentId)?.Items?.FirstOrDefault();

                if (asset != null && asset.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("An asset with that name already exists for this agent");
                }
            }
            else //global asset
            {
                var asset = _assetRepository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))
                .Items?.FirstOrDefault();

                if (asset != null && asset.Id != request.Id)
                {
                    throw new EntityAlreadyExistsException("A global asset already exists with that name");
                }
            }
        }

        public Asset GetSizeInBytes(Asset asset)
        {
            if (asset.Type == "Text")
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.TextValue);
            if (asset.Type == "Number")
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.NumberValue.ToString());
            if (asset.Type == "Json")
                asset.SizeInBytes = System.Text.Encoding.Unicode.GetByteCount(asset.JsonValue);

            return asset;
        }

        //TODO: replace with new FileSystem
        private Asset AddAssetFile(Asset asset, IFormFile file)
        {
            if (file == null)
            {
                throw new EntityOperationException("No file was uploaded");
            }

            long size = file.Length;
            if (size <= 0)
            {
                throw new EntityOperationException($"File size of file {file.FileName} cannot be 0");
            }

            string organizationId = _binaryObjectManager.GetOrganizationId();
            string apiComponent = "AssetAPI";

            BinaryObject binaryObject = new BinaryObject();
            binaryObject.Name = file.FileName;
            binaryObject.Folder = apiComponent;
            binaryObject.CreatedOn = DateTime.UtcNow;
            binaryObject.CreatedBy = _caller.Identity.Name;
            binaryObject.CorrelationEntityId = asset.Id;

            string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

            var existingbinary = _binaryObjectRepository.Find(null, x => x.Folder?.ToLower(null) == binaryObject.Folder.ToLower(null) && x.Name.ToLower(null) == file?.FileName?.ToLower(null) && x.Id != binaryObject.Id)?.Items?.FirstOrDefault();
            if (existingbinary != null)
            {
                throw new EntityAlreadyExistsException("Same file name already exists in the given folder");
            }
            _binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
            _binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
            _binaryObjectRepository.Add(binaryObject);

            asset.BinaryObjectID = binaryObject.Id;
            asset.SizeInBytes = file.Length;

            return asset;
        }
    }
}
