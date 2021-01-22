using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IAutomationManager : IManager
    {
        Task<FileObjectViewModel> Export(string binaryObjectId);
        bool DeleteAutomation(Guid automationId);
        Automation UpdateAutomation(Automation existingAutomation, AutomationViewModel request);
        Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "");
        string GetOrganizationId();
        void AddAutomationVersion(AutomationViewModel automation);
        PaginatedList<AllAutomationsViewModel> GetAutomationsAndAutomationVersions(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        AutomationViewModel GetAutomationView(AutomationViewModel automationView, string id);
    }
}
