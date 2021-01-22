using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Interface for AuditLogRepository
    /// </summary>
    public interface IAuditLogRepository : IEntityRepository<AuditLog>
    {
        public string GetServiceName(AuditLog log);
        public PaginatedList<AuditLogViewModel> FindAllView(Predicate<AuditLogViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
