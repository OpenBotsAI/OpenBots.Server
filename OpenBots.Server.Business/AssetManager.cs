using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.Model;

namespace OpenBots.Server.Business
{
    public class AssetManager : BaseManager, IAssetManager
    {
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
    }
}
