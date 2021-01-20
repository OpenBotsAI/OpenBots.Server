using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileManager : IManager
    {
        public FileViewModel GetFile(string fileId = null);

        public FileViewModel SaveFile(SaveServerFileViewModel request);

        public FileViewModel UpdateFile(UpdateServerFileViewModel request);

        public void DeleteFile(string path);

        public int? GetFolderCount();

        public ServerFolder GetFolder(string name);

        public ServerDrive GetDrive();

        public enum AdapterType
        { }
    }
}
