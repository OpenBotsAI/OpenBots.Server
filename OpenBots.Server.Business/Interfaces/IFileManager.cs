using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileManager : IManager
    {
        public FileFolderViewModel AddFileFolder(FileFolderViewModel request);

        //public void UpdateFile(UpdateServerFileViewModel request);

        //public void DeleteFile(string path);

        public int? GetFolderCount();

        public FileFolderViewModel GetFileFolder(string path);

       public ServerDrive GetDrive();

        //public void DeleteFolder(string path);

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

        public Task<FileFolderViewModel> ExportFileFolder(string id);

        public enum AdapterType
        { }
    }
}
