using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class LimitParallelJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxRunningJobs",
                table: "Schedules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRunningJobs",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Jobs");
        }
    }
}
