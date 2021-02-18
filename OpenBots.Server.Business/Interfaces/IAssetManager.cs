using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.ViewModel.ViewModels;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IAssetManager : IManager
    {
        Asset CreateAgentAsset(AgentAssetViewModel request);
        Asset GetMatchingAsset(string assetName, string assetType);
        void AssetNameAvailability(Asset request);
        Asset CreateAsset(Asset asset, IFormFile file = null, string driveName = null);
        Task<FileFolderViewModel> Export(string id, string driveName = null);
        Asset GetSizeInBytes(Asset asset);
        Asset UpdateAsset(string id, Asset asset);
        Asset UpdateAssetFile(string id, UpdateAssetViewModel request);
        Asset DeleteAsset(string id, string driveName = null);
        Asset PatchAsset(string id, JsonPatchDocument<Asset> request);
        Asset Increment(string id);
        Asset Decrement(string id);
        Asset Add(string id, int value);
        Asset Subtract(string id, int value);
        Asset Append(string id, string value);
    }
}
