using OpenBots.Server.ViewModel.File;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileStorageAdapter
    {
        public object DownloadFile(string downloadInput);

        public void SaveFile(SaveServerFileViewModel viewModel);

        public void UpdateFile(UpdateServerFileViewModel request);

        public void DeleteFile(string path);
    }
}
