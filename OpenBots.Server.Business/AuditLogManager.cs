﻿using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.IO;
using System.IO.Compression;

namespace OpenBots.Server.Business
{
    public class AuditLogManager : BaseManager, IAuditLogManager
    {
        private readonly IAuditLogRepository _repo;

        public AuditLogManager(IAuditLogRepository repo)
        {
            _repo = repo;
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

        public PaginatedList<AuditLogViewModel> GetAuditLogsView(Predicate<AuditLogViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }
    }
}
