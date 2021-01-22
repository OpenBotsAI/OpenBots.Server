using Microsoft.AspNetCore.Http;
using Syncfusion.EJ2.FileManager.Base;
using System.Collections.Generic;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileManager : IManager
    {
        public object DownloadFile(string downloadInput);

        //public void SaveFile(SaveServerFileViewModel request);

        //public void UpdateFile(UpdateServerFileViewModel request);

        //public void DeleteFile(string path);

        //public int? GetFolderCount();

        //public ServerFolder GetFolder(string name);

        //public ServerDrive GetDrive();

        //public void DeleteFolder(string path);

        public object LocalFileStorageOperation(FileManagerDirectoryContent args);

        public FileManagerResponse UploadFile(string path, IList<IFormFile> uploadFiles, string action);

        public object GetImage(FileManagerDirectoryContent args);

        public enum AdapterType
        { }
    }
}
