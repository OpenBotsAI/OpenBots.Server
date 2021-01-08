using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IAuditLogManager : IManager
    {
        string GetAuditLogs(AuditLog[] auditLogs);
        MemoryStream ZipCsv(FileContentResult csvFile);

        public PaginatedList<AuditLogViewModel> GetAuditLogsView(Predicate<AuditLogViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
