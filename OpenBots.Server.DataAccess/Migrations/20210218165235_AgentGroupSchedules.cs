using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AgentGroupSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentGroupId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "AgentGroupId",
                table: "Jobs");

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
