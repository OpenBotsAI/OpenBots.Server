using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IAutomationManager : IManager
    {
        Automation AddAutomation(AutomationViewModel automationViewModel);
        Automation UpdateAutomationFile(string id, AutomationViewModel request);
        Automation UpdateAutomation(string id, AutomationViewModel request);
        Task<FileFolderViewModel> Export(string fileId, string driveId = null);
        void DeleteAutomation(Automation automation);
        void AddAutomationVersion(AutomationViewModel automation);
        PaginatedList<AllAutomationsViewModel> GetAutomationsAndAutomationVersions(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        AutomationViewModel GetAutomationView(AutomationViewModel automationView);
        IEnumerable<AutomationParameter> UpdateAutomationParameters(IEnumerable<AutomationParameter> automationParameters, string automationId);
        IEnumerable<AutomationParameter> AddAutomationParameters(IEnumerable<AutomationParameter> automationParameters, Guid? automationId);
        IEnumerable<AutomationParameter> GetAutomationParameters(Guid? automationId);
        void DeleteExistingParameters(Guid? automationId);
        void CheckParameterNameAvailability(IEnumerable<AutomationParameter> automationParameters);

    }
}
