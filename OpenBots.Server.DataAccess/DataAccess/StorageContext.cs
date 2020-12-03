using OpenBots.Server.Model.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Webhooks;

namespace OpenBots.Server.DataAccess
{
    public partial class StorageContext : DbContext
    {
        public DbSet<LookupValue> LookupValues { get; set; }
        public DbSet<ApplicationVersion> AppVersion { get; set; }
        public DbSet<QueueItemModel> QueueItems { get; set; }
        public DbSet<QueueItemAttachment> QueueItemAttachments { get; set; }
        public DbSet<BinaryObject> BinaryObjects { get; set; }
        public DbSet<AgentModel> Agents { get; set; }
        public DbSet<AgentHeartbeat> AgentHeartbeats { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<ProcessVersion> ProcessVersions { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobParameter> JobParameters { get; set; }
        public DbSet<JobCheckpoint> JobCheckpoints { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<ProcessExecutionLog> ProcessExecutionLogs { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<EmailModel> Emails { get; set; }
        public DbSet<EmailAttachment> EmailAttachments { get; set; }
        public DbSet<IPFencing> IPFencings { get; set; }
        public DbSet<IntegrationEvent> IntegrationEvents { get; set; }
        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }
        public DbSet<IntegrationEventSubscription> IntegrationEventSubscriptions { get; set; }
        public DbSet<IntegrationEventSubscriptionAttempt> IntegrationEventSubscriptionAttempts { get; set; }


        public StorageContext(DbContextOptions<StorageContext> options)
      : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateMembershipModel(modelBuilder);
            CreateIdentityModel(modelBuilder);
            CreateCoreModel(modelBuilder);
        }
        #region core entitites

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
