using OpenBots.Server.Model;
using OpenBots.Server.ViewModel.ViewModels;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IAssetManager : IManager
    {
        Asset CreateAgentAsset(AgentAssetViewModel request);
        Asset GetMatchingAsset(string assetName, string assetType);
        void AssetNameAvailability(Asset request);
        Asset GetSizeInBytes(Asset asset);
    }
}
