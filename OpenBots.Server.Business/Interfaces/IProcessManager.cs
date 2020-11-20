using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IProcessManager : IManager
    {
        Task<FileObjectViewModel> Export(string binaryObjectId);
        bool DeleteProcess(Guid processId);
        //Process AssignProcessProperties(Process request, Guid versionId);
        Process UpdateProcess(Process existingProcess, ProcessViewModel request);
        Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "");
        string GetOrganizationId();
        void AddProcessVersion(ProcessViewModel process);
        PaginatedList<AllProcessesViewModel> GetProcessesAndProcessVersions(Predicate<AllProcessesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        ProcessViewModel GetProcessView(ProcessViewModel processView, string id);
    }
}
