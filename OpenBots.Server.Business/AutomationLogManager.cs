using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenBots.Server.Business
{
    public class AutomationLogManager : BaseManager, IAutomationLogManager
    {
        private readonly IAutomationLogRepository repo;
        public AutomationLogManager(IAutomationLogRepository repo)
        {
            this.repo = repo;
        }

        public string GetJobLogs(AutomationLog[] automationLogs)
        {
            string csvString = "ID,TimeStamp,Level,Message,MachineName,AutomationName,AgentName,JobID";
            foreach (AutomationLog log in automationLogs)
            {

                csvString += Environment.NewLine + string.Join(",", log.Id, log.AutomationLogTimeStamp, log.Level, log.Message, 
                    log.MachineName, log.AutomationName, log.AgentName, log.JobId);
            }

            return csvString;
        }

        public MemoryStream ZipCsv(FileContentResult csvFile)
        {
            var compressedFileStream = new MemoryStream();

            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update))
            {
                var zipEntry = zipArchive.CreateEntry("Logs.csv");

                using (var originalFileStream = new MemoryStream(csvFile.FileContents))
                using (var zipEntryStream = zipEntry.Open())
                {
                    originalFileStream.CopyTo(zipEntryStream);
                }
            }
            return compressedFileStream;
        }
    }
}
