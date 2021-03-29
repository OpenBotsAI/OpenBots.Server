using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileManager : IManager
    {
        List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName = null);

        void UpdateFile(FileFolderViewModel request);

        FileFolderViewModel DeleteFileFolder(string id, string driveName = null);

        void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files);

        void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> file);

        int? GetFileCount(string driveName = null);

        int? GetFolderCount(string driveName = null);

        FileFolderViewModel GetFileFolder(string id, string driveName = null);

        StorageDrive GetDrive(string driveName = null);

        PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

        Task<FileFolderViewModel> ExportFileFolder(string id, string driveName = null);

        FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null);

        FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null);

        FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveName = null);

        FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName = null);

        StorageDrive AddStorageDrive(string driveName);

        Dictionary<Guid?, string> GetDriveNames(string adapterType);
    }
}
