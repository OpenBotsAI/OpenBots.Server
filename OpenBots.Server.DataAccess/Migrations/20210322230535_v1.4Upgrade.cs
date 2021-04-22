using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class v14Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "Schedules");

            migrationBuilder.AddColumn<Guid>(
                name: "AgentGroupId",
                table: "Schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AgentId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AgentGroupId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgentGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentGroupMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutomationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationParameters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("fa264362-998e-473d-8645-e6fdf86bc79f"), "", null, null, "", "A new Drive has been created", "File", false, true, "Files.NewDriveCreated", null, null, null },
                    { new Guid("2c5b29c7-2fed-42b6-afcb-b7d8a41aacb5"), "", null, null, "", "A new AgentGroup has been created", "AgentGroup", false, true, "AgentGroups.NewAgentGroupCreated", null, null, null },
                    { new Guid("e096bb0f-850c-4001-946a-88a7f8692d5a"), "", null, null, "", "An AgentGroup has been updated", "AgentGroup", false, true, "AgentGroups.AgentGroupUpdated", null, null, null },
                    { new Guid("3789f1ae-2693-4ad7-8696-723bd551199f"), "", null, null, "", "An AgentGroup has been deleted", "AgentGroup", false, true, "AgentGroups.AgentGroupDeleted", null, null, null },
                    { new Guid("76910164-6fda-4861-b1b5-7737370a8461"), "", null, null, "", "An Agent has been added to the AgentGroup", "AgentGroup", false, true, "AgentGroups.AgentGroupMemberUpdated", null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentGroupMembers");

            migrationBuilder.DropTable(
                name: "AgentGroups");

            migrationBuilder.DropTable(
                name: "AutomationParameters");

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("2c5b29c7-2fed-42b6-afcb-b7d8a41aacb5"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("3789f1ae-2693-4ad7-8696-723bd551199f"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("76910164-6fda-4861-b1b5-7737370a8461"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("e096bb0f-850c-4001-946a-88a7f8692d5a"));

            migrationBuilder.DeleteData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("fa264362-998e-473d-8645-e6fdf86bc79f"));

            migrationBuilder.DropColumn(
                name: "AgentGroupId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "AgentGroupId",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "Schedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AgentId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
