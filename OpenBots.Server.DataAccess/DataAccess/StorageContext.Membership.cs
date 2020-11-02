using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OpenBots.Server.DataAccess
{
    public partial class StorageContext
    {
        #region Membership

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AccessRequest> AccessRequests { get; set; }

        public DbSet<OrganizationMember> OrganizationMembers { get; set; }

        public DbSet<OrganizationSetting> OrganizationSettings { get; set; }

        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }

        public DbSet<OrganizationUnitMember> OrganizationUnitMembers { get; set; }

        #endregion Membership

        protected static void CreateMembershipModel(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) return;
            CreateOrganizationModel(modelBuilder.Entity<Organization>());
            CreateAccessRequestModel(modelBuilder.Entity<AccessRequest>());
            CreateOrganizationMemberModel(modelBuilder.Entity<OrganizationMember>());
            CreateOrganizationSettingModel(modelBuilder.Entity<OrganizationSetting>());
            CreateOrganizationUnitModel(modelBuilder.Entity<OrganizationUnit>());
            CreateOrganizationUnitMemberModel(modelBuilder.Entity<OrganizationUnitMember>());
        }

        protected static void CreateOrganizationModel(EntityTypeBuilder<Organization> entity)
        {
            if (entity == null) return;
            entity.HasIndex(o => o.Name).IsUnique();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        protected static void CreateAccessRequestModel(EntityTypeBuilder<AccessRequest> entity)
        {
            if (entity == null) return;
            entity.HasOne(a => a.Organization).WithMany(o => o.AccessRequests).OnDelete(DeleteBehavior.Restrict); ;
            entity.HasOne(ar => ar.Person).WithMany().OnDelete(DeleteBehavior.Restrict); ;
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        protected static void CreateOrganizationMemberModel(EntityTypeBuilder<OrganizationMember> entity)
        {
            if (entity == null) return;
            //  entity.HasOne(om => om.Organization).WithMany(o => o.Members).OnDelete(DeleteBehavior.Restrict); ;
            entity.HasOne(om => om.Person).WithMany().OnDelete(DeleteBehavior.Restrict); ;
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        protected static void CreateOrganizationSettingModel(EntityTypeBuilder<OrganizationSetting> entity)
        {
            if (entity == null) return;
            entity.HasOne(os => os.Organization).WithMany(o => o.Settings).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }

        protected static void CreateOrganizationUnitModel(EntityTypeBuilder<OrganizationUnit> entity)
        {
            if (entity == null) return;
            entity.HasIndex(o => new { o.OrganizationId, o.Name}).IsUnique();
            entity.HasOne(u => u.Organization).WithMany(o => o.Units).OnDelete(DeleteBehavior.Restrict); ;
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
            entity.Property(e => e.CanDelete).HasDefaultValueSql("1");
        }

        protected static void CreateOrganizationUnitMemberModel(EntityTypeBuilder<OrganizationUnitMember> entity)
        {
            if (entity == null) return;
            entity.HasOne(oum => oum.OrganizationUnit).WithMany(ou => ou.Members).OnDelete(DeleteBehavior.Restrict); ;
            entity.HasOne(oum => oum.Person).WithMany().OnDelete(DeleteBehavior.Restrict); ;
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("getutcdate()");
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
        }
    }
}
