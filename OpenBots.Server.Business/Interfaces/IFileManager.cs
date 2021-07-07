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
        List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveId);

        FileFolderViewModel UpdateFile(FileFolderViewModel request);

        FileFolderViewModel DeleteFileFolder(string id, string driveId, string type);

        void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files);

        void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> file);

        int? GetFileCount(string driveId);

        int? GetFolderCount(string driveId);

        FileFolderViewModel GetFileFolder(string id, string driveId, string type);

        StorageDrive GetDriveByName(string driveName);

        StorageDrive GetDriveById(string driveId);

        PaginatedList<FileFolderViewModel> GetFilesFolders(string driveId, bool? isFile, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null);

        Task<FileFolderViewModel> ExportFileFolder(string id, string driveId);

        FileFolderViewModel RenameFileFolder(string id, string name, string driveId, string type);

        FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);

        FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);

        FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName);

        StorageDrive AddStorageDrive(StorageDrive drive);

        StorageDrive UpdateDrive(string id, StorageDrive drive, string organizationId);

        Dictionary<Guid?, string> GetDriveNames(string adapterType);

        string GetShortPath(string path);

        void CheckDefaultDrive(StorageDrive drive, Guid? organizationId);
    }
}
