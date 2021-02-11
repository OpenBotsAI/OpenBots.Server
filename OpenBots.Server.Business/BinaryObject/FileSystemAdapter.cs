﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace OpenBots.Server.Business
{
    public class FileSystemAdapter : IFileSystemAdapter
    {
        private readonly IDirectoryManager _directoryManager;

        public FileSystemAdapter(IDirectoryManager directoryManager)
        {
            _directoryManager = directoryManager;
        }

        public string SaveFile(IFormFile file, string path, string organizationId, string apiComponent, string binaryObjectId)
        {
            //save file to OpenBots.Server.Web/BinaryObjects/{organizationId}/{apiComponent}/{binaryObjectId}
            apiComponent = apiComponent ?? string.Empty;
            var target = Path.Combine(path, organizationId, apiComponent);

            if (!_directoryManager.Exists(target))
            {
                _directoryManager.CreateDirectory(target);
            }

            var filePath = Path.Combine(target, binaryObjectId);

            if (file.Length <= 0 || file.Equals(null)) return "No file exists.";
            if (file.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(filePath);
            }
            return binaryObjectId;
        }

        public void ConvertToBinaryObject(string filePath)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);
            System.IO.File.WriteAllBytes(filePath, bytes);
        }

        public void UpdateFile(IFormFile file, string path, string organizationId, string apiComponent, Guid binaryObjectId)
        {
            //update file to OpenBots.Server.Web/BinaryObjects/{organizationId}/{apiComponent}/{binaryObjectId}
            apiComponent = apiComponent ?? string.Empty;
            var target = Path.Combine(path, organizationId, apiComponent);

            if (!_directoryManager.Exists(target))
            {
                _directoryManager.CreateDirectory(target);
            }

            var filePath = Path.Combine(target, binaryObjectId.ToString());

            if (file.Length > 0)
            {
                System.IO.File.Delete(filePath);
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(filePath);
            }
        }
    }
}
