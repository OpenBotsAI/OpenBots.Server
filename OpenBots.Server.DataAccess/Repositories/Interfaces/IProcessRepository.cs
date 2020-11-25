using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Interface for ProcessRepository
    /// </summary>
    public interface IProcessRepository : IEntityRepository<Process>
    {
        PaginatedList<AllProcessesViewModel> FindAllView(Predicate<AllProcessesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
