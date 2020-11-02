using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IAuditLogManager : IManager
    {
        string GetAuditLogs(AuditLog[] auditLogs);
        MemoryStream ZipCsv(FileContentResult csvFile);
    }
}
