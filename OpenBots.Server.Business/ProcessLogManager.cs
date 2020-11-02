using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenBots.Server.Business
{
    public class ProcessLogManager : BaseManager, IProcessLogManager
    {
        private readonly IProcessLogRepository repo;
        public ProcessLogManager(IProcessLogRepository repo)
        {
            this.repo = repo;
        }

        public string GetJobLogs(ProcessLog[] processLogs)
        {
            string csvString = "ID,TimeStamp,Level,Message,MachineName,ProcessName,AgentName,JobID";
            foreach (ProcessLog log in processLogs)
            {

                csvString += Environment.NewLine + string.Join(",", log.Id, log.ProcessLogTimeStamp, log.Level, log.Message, 
                    log.MachineName, log.ProcessName, log.AgentName, log.JobId);
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
