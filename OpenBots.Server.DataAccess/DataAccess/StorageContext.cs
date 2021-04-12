﻿using OpenBots.Server.Model.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Webhooks;
using System;
using OpenBots.Server.Model.File;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace OpenBots.Server.DataAccess
{
    public partial class StorageContext : DbContext
    {
        public DbSet<LookupValue> LookupValues { get; set; }
        public DbSet<ApplicationVersion> AppVersion { get; set; }
        public DbSet<TimeZoneId> TimeZoneIds { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }
        public DbSet<QueueItemAttachment> QueueItemAttachments { get; set; }
        public DbSet<BinaryObject> BinaryObjects { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<AgentGroup> AgentGroups { get; set; }
        public DbSet<AgentGroupMember> AgentGroupMembers { get; set; }
        public DbSet<AgentHeartbeat> AgentHeartbeats { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Automation> Automations { get; set; }
        public DbSet<AutomationVersion> AutomationVersions { get; set; }
        public DbSet<AutomationExecutionLog> AutomationExecutionLogs { get; set; }
        public DbSet<AutomationLog> AutomationLogs { get; set; }
        public DbSet<AutomationParameter> AutomationParameters { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobParameter> JobParameters { get; set; }
        public DbSet<JobCheckpoint> JobCheckpoints { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleParameter> ScheduleParameters { get; set; }
        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailAttachment> EmailAttachments { get; set; }
        public DbSet<IPFencing> IPFencings { get; set; }
        public DbSet<IntegrationEvent> IntegrationEvents { get; set; }
        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }
        public DbSet<IntegrationEventSubscription> IntegrationEventSubscriptions { get; set; }
        public DbSet<IntegrationEventSubscriptionAttempt> IntegrationEventSubscriptionAttempts { get; set; }
        public DbSet<StorageDrive> StorageDrives { get; set; }
        public DbSet<StorageFolder> StorageFolders { get; set; }
        public DbSet<StorageFile> StorageFiles { get; set; }
        public DbSet<FileAttribute> FileAttributes { get; set; }


        public StorageContext(DbContextOptions<StorageContext> options)
      : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            SeedIntegrationEvents(modelBuilder);
            SeedTimeZoneIdModel(modelBuilder);
            CreateMembershipModel(modelBuilder);
            CreateIdentityModel(modelBuilder);
            CreateCoreModel(modelBuilder);
        }
        #region core entitites

        protected void SeedIntegrationEvents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntegrationEvent>().HasData(
            new IntegrationEvent { Id = new Guid("744ba6f9-161f-41dc-b76e-c1602fc65d1b"), Description = "A Queue has been updated", EntityType = "Queue", IsSystem = true, IsDeleted = false, Name = "Queues.QueueUpdated" },
            new IntegrationEvent { Id = new Guid("b00eeecd-5729-4f82-9cd2-dcfafd946965"), Description = "A Queue has been deleted", EntityType = "Queue", IsSystem = true, IsDeleted = false, Name = "Queues.QueueDeleted" },
            new IntegrationEvent { Id = new Guid("e9f64119-edbf-4779-a796-21ad59f76534"), Description = "A new Queue has been created", EntityType = "Queue", IsSystem = true, IsDeleted = false, Name = "Queues.NewQueueCreated" },
            new IntegrationEvent { Id = new Guid("0719a4c3-2143-4b9a-92ae-8b5a93075b98"), Description = "A QueueItem has been updated", EntityType = "QueueItem", IsSystem = true, IsDeleted = false, Name = "QueueItems.QueueItemUpdated" },
            new IntegrationEvent { Id = new Guid("860689af-fd19-44ba-a5c7-53f6fed92065"), Description = "A QueueItem has been deleted", EntityType = "QueueItem", IsSystem = true, IsDeleted = false, Name = "QueueItems.QueueItemDeleted" },
            new IntegrationEvent { Id = new Guid("30a8dcb9-10cf-43c6-a08f-b45fe2125dae"), Description = "A new QueueItem has been created", EntityType = "QueueItem", IsSystem = true, IsDeleted = false, Name = "QueueItems.NewQueueItemCreated" },
            new IntegrationEvent { Id = new Guid("06dd9940-a483-4a21-9551-cf2e32eeccae"), Description = "A new Job has been created", EntityType = "Job", IsSystem = true, IsDeleted = false, Name = "Jobs.NewJobCreated" },
            new IntegrationEvent { Id = new Guid("9d8e576a-a69d-43cf-bbc9-18103105d0a0"), Description = "A Job has been updated", EntityType = "Job", IsSystem = true, IsDeleted = false, Name = "Jobs.JobUpdated" },
            new IntegrationEvent { Id = new Guid("82b8d08d-5ae2-4031-bdf8-5fba5597ac4b"), Description = "A Job has been deleted", EntityType = "Job", IsSystem = true, IsDeleted = false, Name = "Jobs.JobsDeleted" },
            new IntegrationEvent { Id = new Guid("04cf6a7a-ca72-48bc-887f-666ef580d198"), Description = "A new File has been created", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.NewFileCreated" },
            new IntegrationEvent { Id = new Guid("3ff9b456-7832-4499-b263-692c021e7d80"), Description = "A File has been updated", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.FileUpdated" },
            new IntegrationEvent { Id = new Guid("32d63e9d-aa6e-481f-b928-541ddf979bdf"), Description = "A File has been deleted", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.FileDeleted" },
            new IntegrationEvent { Id = new Guid("76f6ab13-c430-46ad-b859-3d2dfd802e84"), Description = "A new Credential has been created", EntityType = "Credential", IsSystem = true, IsDeleted = false, Name = "Credentials.NewCredentialCreated" },
            new IntegrationEvent { Id = new Guid("efd1d688-1881-4d5e-aed7-81528d54d7ef"), Description = "A Credential has been updated", EntityType = "Credential", IsSystem = true, IsDeleted = false, Name = "Credentials.CredentialUpdated" },
            new IntegrationEvent { Id = new Guid("ecced501-9c35-4b37-a7b2-b6b901f91234"), Description = "A Credential has been deleted", EntityType = "Credential", IsSystem = true, IsDeleted = false, Name = "Credentials.CredentialDeleted" },
            new IntegrationEvent { Id = new Guid("93416738-3284-4bb0-869e-e2f191446b44"), Description = "A new Process has been created", EntityType = "Automation", IsSystem = true, IsDeleted = false, Name = "Automations.NewAutomationCreated" },
            new IntegrationEvent { Id = new Guid("8437fa1f-777a-4905-a169-feb32214c0c8"), Description = "A Process has been updated", EntityType = "Automation", IsSystem = true, IsDeleted = false, Name = "Automations.AutomationUpdated" },
            new IntegrationEvent { Id = new Guid("90f9f691-90e5-41d0-9b2c-1e8437bc85d3"), Description = "A Process has been deleted", EntityType = "Automation", IsSystem = true, IsDeleted = false, Name = "Automations.AutomationDeleted" },
            new IntegrationEvent { Id = new Guid("f1b111cc-1f26-404d-827c-e30305c2ecc4"), Description = "A new Asset has been created", EntityType = "Asset", IsSystem = true, IsDeleted = false, Name = "Assets.NewAssetCreated" },
            new IntegrationEvent { Id = new Guid("4ce67735-2edc-4b7f-849a-5575740a496f"), Description = "An Asset has been updated", EntityType = "Asset", IsSystem = true, IsDeleted = false, Name = "Assets.AssetUpdated" },
            new IntegrationEvent { Id = new Guid("6e0c741c-34b0-471e-a491-c7ec61782e94"), Description = "An Asset has been deleted", EntityType = "Asset", IsSystem = true, IsDeleted = false, Name = "Assets.AssetDeleted" },
            new IntegrationEvent { Id = new Guid("35fd2aa3-6c77-4995-9ed8-9b262e5afdfc"), Description = "An Agent has reported an unhealthy status", EntityType = "Agent", IsSystem = true, IsDeleted = false, Name = "Agents.UnhealthyReported" },
            new IntegrationEvent { Id = new Guid("6ce8b3da-0373-4da2-bc77-ea845212855d"), Description = "A new agent has been created", EntityType = "Agent", IsSystem = true, IsDeleted = false, Name = "Agents.NewAgentCreated" },
            new IntegrationEvent { Id = new Guid("2b4bd195-62ac-4111-97ca-d6df6dd3f0fb"), Description = "An Agent has been updated", EntityType = "Agent", IsSystem = true, IsDeleted = false, Name = "Agents.AgentUpdated" },
            new IntegrationEvent { Id = new Guid("6ce0bb0e-cda1-49fa-a9e4-b67d904f826e"), Description = "An Agent has been deleted", EntityType = "Agent", IsSystem = true, IsDeleted = false, Name = "Agents.AgentDeleted" },
            new IntegrationEvent { Id = new Guid("53b4365e-d103-4e74-a72c-294d670abdbd"), Description = "A new Folder has been created", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.NewFolderCreated" },
            new IntegrationEvent { Id = new Guid("d10616c6-53c4-4137-8cd0-70a5c7409938"), Description = "A Folder has been updated", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.FolderUpdated" },
            new IntegrationEvent { Id = new Guid("e4a9ceaa-88e2-4c03-a203-7a419749c613"), Description = "A Folder has been deleted", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.FolderDeleted" },
            new IntegrationEvent { Id = new Guid("fa264362-998e-473d-8645-e6fdf86bc79f"), Description = "A new Drive has been created", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.NewDriveCreated" },
            new IntegrationEvent { Id = new Guid("513bb79b-3f2e-4846-a804-2c5b9a6792d0"), Description = "Local Drive has been updated", EntityType = "File", IsSystem = true, IsDeleted = false, Name = "Files.DriveUpdated" },
            new IntegrationEvent { Id = new Guid("2c5b29c7-2fed-42b6-afcb-b7d8a41aacb5"), Description = "A new AgentGroup has been created", EntityType = "AgentGroup", IsSystem = true, IsDeleted = false, Name = "AgentGroups.NewAgentGroupCreated" },
            new IntegrationEvent { Id = new Guid("e096bb0f-850c-4001-946a-88a7f8692d5a"), Description = "An AgentGroup has been updated", EntityType = "AgentGroup", IsSystem = true, IsDeleted = false, Name = "AgentGroups.AgentGroupUpdated" },
            new IntegrationEvent { Id = new Guid("3789f1ae-2693-4ad7-8696-723bd551199f"), Description = "An AgentGroup has been deleted", EntityType = "AgentGroup", IsSystem = true, IsDeleted = false, Name = "AgentGroups.AgentGroupDeleted" },
            new IntegrationEvent { Id = new Guid("76910164-6fda-4861-b1b5-7737370a8461"), Description = "An Agent has been added to the AgentGroup", EntityType = "AgentGroup", IsSystem = true, IsDeleted = false, Name = "AgentGroups.AgentGroupMemberUpdated" }
            );
        }

        protected void SeedTimeZoneIdModel(ModelBuilder modelBuilder)
        {
            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; //was AbsolutePath but didn't work with spaces according to comments
            var directoryName = Path.GetDirectoryName(absolutePath);
            using (StreamReader r = new StreamReader($@"{directoryName}\DataAccess\Windows-Linux-TimeZone.json"))
            {
                string json = r.ReadToEnd();
                dynamic idArray = JsonConvert.DeserializeObject(json);

                foreach (var item in idArray)
                {
                    modelBuilder.Entity<TimeZoneId>()
                        .HasData(new TimeZoneId { WindowsTimeZone = item.Windows, LinuxTimeZone = item.Linux, IsDeleted = false});
                }
            }
        }

        protected void CreateCoreModel(ModelBuilder modelBuilder)
        {
            CreateLookupValueModel(modelBuilder.Entity<LookupValue>());
            CreateApplicationVersionModel(modelBuilder.Entity<ApplicationVersion>());
        }

        protected void CreateLookupValueModel(EntityTypeBuilder<LookupValue> entity)
        {
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        protected void CreateApplicationVersionModel(EntityTypeBuilder<ApplicationVersion> entity)
        {
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        #endregion
    }
}
