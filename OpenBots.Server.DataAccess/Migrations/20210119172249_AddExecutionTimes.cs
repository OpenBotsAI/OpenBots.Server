using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddExecutionTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HTTP_Max_RetryCount",
                table: "IntegrationEventSubscriptions",
                newName: "Max_RetryCount");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ExecutionTime",
                table: "Jobs",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AverageSuccessfulExecution",
                table: "Automations",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AverageUnSuccessfulExecution",
                table: "Automations",
                type: "time",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionTime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AverageSuccessfulExecution",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "AverageUnSuccessfulExecution",
                table: "Automations");

            migrationBuilder.RenameColumn(
                name: "Max_RetryCount",
                table: "IntegrationEventSubscriptions",
                newName: "HTTP_Max_RetryCount");
        }
    }
}
