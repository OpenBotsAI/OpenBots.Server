using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface ILocalFileStorageAdapter : IFileStorageAdapter
    {
        PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        FileFolderViewModel GetFileFolderViewModel(string id, string driveName);
        FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName);
        Dictionary<Guid?, string> GetDriveNames(string adapterType);
        StorageDrive GetDriveByName(string name);
        int? GetFileCount(string driveName);
        int? GetFolderCount(string driveName);
        List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName);
        FileFolderViewModel SaveFile(FileFolderViewModel request, IFormFile file, StorageDrive drive);
        void UpdateFile(FileFolderViewModel request);
        Task<FileFolderViewModel> ExportFile(string id, string driveName);
        FileFolderViewModel DeleteFileFolder(string id, string driveName = null);
        FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null);
        FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null);
        FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveName = null);
        void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files);
        void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> files);
        StorageDrive AddStorageDrive(string driveName);
        string GetShortPath(string path);
    }
}
