using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Interface for AutomationRepository
    /// </summary>
    public interface IAutomationRepository : IEntityRepository<Automation>
    {
        PaginatedList<AllAutomationsViewModel> FindAllView(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
