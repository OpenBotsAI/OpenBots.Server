using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IBinaryObjectManager: IManager
    {
        string Upload(IFormFile file, string organizationId, string apiComponent, string binaryObjectId);

        void SaveEntity(IFormFile file, string filePath, BinaryObject binaryObject, string apiComponent, string organizationId);

        Task<FileObjectViewModel> Download(string binaryObjectId);

        void Update(IFormFile file, string organizationId, string apiComponent, Guid binaryObjectId);

        Task<string> UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name);

        string GetOrganizationId();
    }
}
