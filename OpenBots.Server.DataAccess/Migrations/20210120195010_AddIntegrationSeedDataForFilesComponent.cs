using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddIntegrationSeedDataForFilesComponent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("53b4365e-d103-4e74-a72c-294d670abdbd"), "", null, null, "", "A new Folder has been created", "File", false, true, "Files.NewFolderCreated", null, null, null },
                    { new Guid("d10616c6-53c4-4137-8cd0-70a5c7409938"), "", null, null, "", "A Folder has been updated", "File", false, true, "Files.FolderUpdated", null, null, null },
                    { new Guid("e4a9ceaa-88e2-4c03-a203-7a419749c613"), "", null, null, "", "A Folder has been deleted", "File", false, true, "Files.FolderDeleted", null, null, null },
                    { new Guid("513bb79b-3f2e-4846-a804-2c5b9a6792d0"), "", null, null, "", "Local Drive has been updated", "File", false, true, "Files.DriveUpdated", null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("513bb79b-3f2e-4846-a804-2c5b9a6792d0"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("53b4365e-d103-4e74-a72c-294d670abdbd"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("d10616c6-53c4-4137-8cd0-70a5c7409938"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("e4a9ceaa-88e2-4c03-a203-7a419749c613"));
        }
    }
}
