using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RemoveScheduleAgentName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "Schedules");

            migrationBuilder.UpdateData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("76910164-6fda-4861-b1b5-7737370a8461"),
                column: "Name",
                value: "AgentGroups.AgentGroupMemberUpdated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "Schedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IntegrationEvents",
                keyColumn: "Id",
                keyValue: new Guid("76910164-6fda-4861-b1b5-7737370a8461"),
                column: "Name",
                value: "AgentGroups.AgentGroupMemberCreated");
        }
    }
}
