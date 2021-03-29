using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileStorageAdapter
    {
        public Task<FileFolderViewModel> ExportFile(string id, string driveName = null);

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

        public FileFolderViewModel SaveFile(FileFolderViewModel viewModel, IFormFile file, StorageDrive drive);

        public void UpdateFile(FileFolderViewModel request);

        public FileFolderViewModel DeleteFileFolder(string id, string driveName = null);

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null);

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null);

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveName = null);
    }
}
