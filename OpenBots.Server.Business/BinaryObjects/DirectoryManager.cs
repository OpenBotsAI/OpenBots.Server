using System.IO;

namespace OpenBots.Server.Business
{
    public class DirectoryManager : IDirectoryManager
    {
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public void Delete(string path)
        {
            Directory.Delete(path);
        }
    }
}