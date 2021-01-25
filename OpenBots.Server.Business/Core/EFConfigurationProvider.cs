using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBots.Server.DataAccess;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.File;
using OpenBots.Server.Model.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenBots.Server.Business.File.FileManager;

namespace OpenBots.Server.Business
{
    public class EFConfigurationProvider : ConfigurationProvider
    {
        public EFConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
        {
            OptionsAction = optionsAction;
        }

        Action<DbContextOptionsBuilder> OptionsAction;

        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<StorageContext>();

            OptionsAction(builder);

            using (var dbContext = new StorageContext(builder.Options))
            {
                dbContext.Database.EnsureCreated();

                Data = !dbContext.ConfigurationValues.Any()
                    ? CreateAndSaveDefaultValues(dbContext)
                    : dbContext.ConfigurationValues.ToDictionary(c => c.Name, c => c.Value);

                //create server drive
                ServerDrive drive = dbContext.ServerDrives.FirstOrDefault();
                if (drive == null)
                {
                    Organization organization = dbContext.Organizations.FirstOrDefault();
                    if (organization != null)
                    {
                        Guid? organizationId = organization.Id;
                        dbContext.ServerDrives.Add(new ServerDrive { Id = new Guid("37a01356-7514-47a2-96ce-986faadd628e"), FileStorageAdapterType = AdapterType.LocalFileStorageAdapter.ToString(), Name = "ServerDrive", OrganizationId = organizationId, StorageSizeInBytes = 0, IsDeleted = false });
                    }

                }
                dbContext.SaveChanges();
            }
        }

        private static IDictionary<string, string> CreateAndSaveDefaultValues(StorageContext dbContext)
        {
            var configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "BinaryObjects:Adapter", "FileSystemAdapter" },
                { "BinaryObjects:StorageProvider", "FileSystem.Default" },
                { "BinaryObjects:Path", "BinaryObjects"},
                { "Queue.Global:DefaultMaxRetryCount", "3" },
                { "App:EnableSwagger", "true"},
                { "App:MaxExportRecords", "100"},
                { "App:MaxReturnRecords", "100"},
            };

            foreach (var value in configValues)
            {
                var configValue = new ConfigurationValue()
                {
                    Id = Guid.NewGuid(),
                    Name = value.Key,
                    Value = value.Value,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "OpenBots Server",
                    IsDeleted = false,
                    Timestamp = new byte[1]
                };
                dbContext.ConfigurationValues.Add(configValue);

                var auditLog = new AuditLog()
                {
                    ChangedFromJson = null,
                    ChangedToJson = JsonConvert.SerializeObject(configValue),
                    CreatedBy = "OpenBots Server",
                    CreatedOn = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    IsDeleted = false,
                    MethodName = "Add",
                    ServiceName = "OpenBots.Server.Model.Configuration.ConfigurationValue",
                    Timestamp = new byte[1],
                    ParametersJson = "",
                    ExceptionJson = "",
                    ObjectId = configValue.Id
                };
                dbContext.AuditLogs.Add(auditLog);
            }

            dbContext.SaveChanges();

            return configValues;
        }
    }
}