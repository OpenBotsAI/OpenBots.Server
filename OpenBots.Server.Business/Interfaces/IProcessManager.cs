using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IProcessManager : IManager
    {
        Task<FileObjectViewModel> Export(string binaryObjectId);

        bool DeleteProcess(Guid processId);

        Process AssignProcessProperties(Process request, Guid versionId);

        Process UpdateProcess(Process requestObj, ProcessViewModel request, Guid versionId);

        Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "");

        string GetOrganizationId();
    }
}
