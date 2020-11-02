using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IBlobStorageAdapter
    {
        void SaveEntity(IFormFile file, string filePath, BinaryObject binaryObject, string apiComponent, string organizationId, string storageProvider, string hash);
        Task<FileObjectViewModel> FetchFile(string binaryObjectId);
        Task<string> UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name, string hash);
    }
}
