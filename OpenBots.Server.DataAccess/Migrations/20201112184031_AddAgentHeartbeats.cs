using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddAgentHeartbeats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHealthy",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedMessage",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedOn",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedStatus",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedWork",
                table: "Agents");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "AgentHeartbeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastReportedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReportedStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastReportedWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastReportedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_AgentHeartbeats", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentHeartbeats");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsHealthy",
                table: "Agents",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedMessage",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReportedOn",
                table: "Agents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedStatus",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedWork",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
