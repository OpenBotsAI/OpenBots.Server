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
using System.IO;
using System.Linq;

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
                var drive = dbContext.ServerDrives.Any();
                if (!drive)
                {
                    Organization organization = dbContext.Organizations.FirstOrDefault();

                    if (organization != null)
                    {
                        Guid? organizationId = organization.Id;
                        Guid driveId = new Guid("37a01356-7514-47a2-96ce-986faadd628e");
                        string storagePath = "Files";
                        string emailAttachments = "Email Attachments";
                        string queueItemAttachments = "Queue Item Attachments";
                        string automations = "Automations";
                        string assets = "Assets";
                        dbContext.ServerDrives.Add(new ServerDrive { Id = driveId, FileStorageAdapterType = "LocalFileStorage", Name = storagePath, OrganizationId = organizationId, StorageSizeInBytes = 0, IsDeleted = false, StoragePath = storagePath });
                        dbContext.ServerFolders.Add(new ServerFolder { Id = new Guid("eea9B112-4eaf-4733-b67b-b71fea62ef06"), Name = emailAttachments, OrganizationId = organizationId, ParentFolderId = driveId, SizeInBytes = 0, StorageDriveId = driveId, StoragePath = Path.Combine(storagePath, emailAttachments) });
                        dbContext.ServerFolders.Add(new ServerFolder { Id = new Guid("e5981bba-dbbf-469f-b2de-5f30f8a3e517"), Name = queueItemAttachments, OrganizationId = organizationId, ParentFolderId = driveId, SizeInBytes = 0, StorageDriveId = driveId, StoragePath = Path.Combine(storagePath, queueItemAttachments) });
                        dbContext.ServerFolders.Add(new ServerFolder { Id = new Guid("7b21c237-f374-4f67-8051-aae101527611"), Name = assets, OrganizationId = organizationId, ParentFolderId = driveId, SizeInBytes = 0, StorageDriveId = driveId, StoragePath = Path.Combine(storagePath, assets) });
                        dbContext.ServerFolders.Add(new ServerFolder { Id = new Guid("5ecd59f0-d2d2-43de-a441-b019432469a6"), Name = automations, OrganizationId = organizationId, ParentFolderId = driveId, SizeInBytes = 0, StorageDriveId = driveId, StoragePath = Path.Combine(storagePath, automations) });

                        //add default server drive
                        Directory.CreateDirectory(storagePath);

                        //add component folders
                        List<string> componentList = new List<string>() { emailAttachments, queueItemAttachments, automations, assets };
                        foreach (var component in componentList)
                        {
                            Directory.CreateDirectory(Path.Combine(storagePath, component));
                        }
                    }
                }
                dbContext.SaveChanges();
            }
        }

        private static IDictionary<string, string> CreateAndSaveDefaultValues(StorageContext dbContext)
        {
            var configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Queue.Global:DefaultMaxRetryCount", "3" },
                { "App:EnableSwagger", "true"},
                { "App:MaxExportRecords", "100"},
                { "App:MaxReturnRecords", "100"},
                { "Files:Adapter", "LocalFileStorageAdapter" },
                { "Files:StorageProvider", "LocalFileStorage" }
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