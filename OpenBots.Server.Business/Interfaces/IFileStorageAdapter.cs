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
        public Task<FileFolderViewModel> ExportFile(string id, string driveId);

        public PaginatedList<FileFolderViewModel> GetFilesFolders(string driveId, bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null);

        public FileFolderViewModel SaveFile(FileFolderViewModel viewModel, IFormFile file, StorageDrive drive);

        public Task<FileFolderViewModel> UpdateFile(FileFolderViewModel request);

        public FileFolderViewModel DeleteFileFolder(string id, string driveId, string type);

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveId, string type);

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);
    }
}
