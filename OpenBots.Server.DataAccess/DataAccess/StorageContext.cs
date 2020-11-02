using OpenBots.Server.Model.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess
{
    public partial class StorageContext : DbContext
    {
        public DbSet<LookupValue> LookupValues { get; set; }
        public DbSet<ApplicationVersion> AppVersion { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }
        public DbSet<BinaryObject> BinaryObjects { get; set; }
        public DbSet<AgentModel> Agents { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<ProcessExecutionLog> ProcessExecutionLogs { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<ConfigurationValue> ConfigurationValues { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }

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
            InitializeSeedData(modelBuilder);
        }

        private void InitializeSeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConfigurationValue>()
                .HasData(
                    new ConfigurationValue { Name = "BinaryObjects:Adapter", Value = "FileSystemAdapter" },
                    new ConfigurationValue { Name = "BinaryObjects:Path", Value = "BinaryObjects" },
                    new ConfigurationValue { Name = "BinaryObjects:StorageProvider", Value = "FileSystem.Default" },
                    new ConfigurationValue { Name = "Queue.Global:DefaultMaxRetryCount", Value = "2" },
                    new ConfigurationValue { Name = "App:MaxExportRecords", Value = "100" },
                    new ConfigurationValue { Name = "App:MaxReturnRecords", Value = "100" },
                    new ConfigurationValue { Name = "App:EnableSwagger", Value = "true" }
                    );
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
