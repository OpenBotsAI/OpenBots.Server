using OpenBots.Server.ViewModel.File;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileStorageAdapter
    {
        public FileViewModel GetFile(string path);

        public FileViewModel SaveFile(SaveServerFileViewModel request);

        public FileViewModel UpdateFile(UpdateServerFileViewModel request);

        public void DeleteFile(string path);
    }
}
