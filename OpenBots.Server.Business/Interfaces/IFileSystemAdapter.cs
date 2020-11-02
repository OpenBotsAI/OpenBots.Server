using Microsoft.AspNetCore.Http;
using System;

namespace OpenBots.Server.Business
{
    public interface IFileSystemAdapter
    {
        string SaveFile(IFormFile file, string path, string organizationId, string apiComponent, string binaryObjectId);

        void ConvertToBinaryObject(string filePath);

        void UpdateFile(IFormFile file, string path, string organizationId, string apiComponent, Guid binaryObjectId);

    }
}
