using Microsoft.AspNetCore.Http;
using System;

namespace OpenBots.Server.ViewModel.File
{
    public class UpdateServerFileViewModel
    {
        public Guid? Id { get; set; }
        public Guid? StorageFolderId { get; set; }
        public string StoragePath { get; set; }
        public string StorageProvider { get; set; }
        public long SizeInBytes { get; set; }
        public IFormFile File { get; set; }
    }
}
