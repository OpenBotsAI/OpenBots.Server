using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenBots.Server.Business
{
    public class AuditLogManager : BaseManager, IAuditLogManager
    {
        private readonly IAuditLogRepository repo;

        public AuditLogManager(IAuditLogRepository repo)
        {
            this.repo = repo;
        }

        public string GetAuditLogs(AuditLog[] auditLogs)
        {
            string csv = "ObjectId, Service, Method, Created By, Created On";
            foreach (AuditLog log in auditLogs)
            {
                if (log.ObjectId == null || !log.ObjectId.HasValue) { log.ObjectId = new Guid("00000000-0000-0000-0000-000000000000"); }
                if (log.CreatedBy == null || log.CreatedBy.Length == 0) { log.CreatedBy = "no user assigned"; }

                csv += Environment.NewLine + string.Join(",", log.ObjectId, log.ServiceName, log.MethodName, log.CreatedBy, log.CreatedOn);
            }

            return csv;
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
