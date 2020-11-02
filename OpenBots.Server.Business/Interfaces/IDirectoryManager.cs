using System.IO;

namespace OpenBots.Server.Business
{
    public interface IDirectoryManager
    {
        DirectoryInfo CreateDirectory(string path);

        bool Exists(string path);
    }
}
