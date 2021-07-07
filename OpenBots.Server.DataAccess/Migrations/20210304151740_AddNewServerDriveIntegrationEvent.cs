using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddNewServerDriveIntegrationEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("fa264362-998e-473d-8645-e6fdf86bc79f"), "", null, null, "", "A new Drive has been created", "File", false, true, "Files.NewDriveCreated", null, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("fa264362-998e-473d-8645-e6fdf86bc79f"));
        }
    }
}
