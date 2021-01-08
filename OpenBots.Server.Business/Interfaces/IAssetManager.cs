using OpenBots.Server.Model;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IAssetManager : IManager
    {
        Asset GetSizeInBytes(Asset asset);
    }
}
