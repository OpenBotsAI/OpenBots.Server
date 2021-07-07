using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface ILocalFileStorageAdapter : IFileStorageAdapter
    {
        PaginatedList<FileFolderViewModel> GetFilesFolders(string driveId, bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null);
        FileFolderViewModel GetFileFolderViewModel(string id, string driveId, string type);
        FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveId);
        Dictionary<Guid?, string> GetDriveNames(string adapterType);
        StorageDrive GetDriveByName(string name);
        int? GetFileCount(string driveId);
        int? GetFolderCount(string driveId);
        List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveId);
        FileFolderViewModel SaveFile(FileFolderViewModel request, IFormFile file, StorageDrive drive);
        Task<FileFolderViewModel> UpdateFile(FileFolderViewModel request);
        Task<FileFolderViewModel> ExportFile(string id, string driveId);
        FileFolderViewModel DeleteFileFolder(string id, string driveId, string type);
        FileFolderViewModel RenameFileFolder(string id, string name, string driveId, string type);
        FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);
        FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveId, string type);
        void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files);
        void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> files);
        string GetShortPath(string path);
        StorageDrive GetDriveById(Guid? id);
    }
}
