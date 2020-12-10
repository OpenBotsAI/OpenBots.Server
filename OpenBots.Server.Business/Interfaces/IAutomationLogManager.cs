using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IAutomationLogManager : IManager
    {
        string GetJobLogs(AutomationLog[] automationLogs);
        MemoryStream ZipCsv(FileContentResult csvFile);
    }
}
