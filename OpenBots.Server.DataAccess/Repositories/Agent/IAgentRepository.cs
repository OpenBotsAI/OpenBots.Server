using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAgentRepository : IEntityRepository<Agent>
    {
        PaginatedList<AllAgentsViewModel> FindAllView(Predicate<AllAgentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
       
        AgentViewModel GetAgentDetailById(string id);
    }
}
