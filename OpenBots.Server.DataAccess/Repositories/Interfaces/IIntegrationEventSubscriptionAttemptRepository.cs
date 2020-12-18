using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IIntegrationEventSubscriptionAttemptRepository : IEntityRepository<IntegrationEventSubscriptionAttempt>
    {
        PaginatedList<SubscriptionAttemptViewModel> FindAllView(Predicate<SubscriptionAttemptViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
