using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IProcessLogManager : IManager
    {
        string GetJobLogs(ProcessLog[] processLogs);
        MemoryStream ZipCsv(FileContentResult csvFile);
    }
}
