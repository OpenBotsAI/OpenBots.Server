using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RenameExecutionTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropColumn(
                name: "ExecutionTime",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AverageSuccessfulExecution",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "AverageUnSuccessfulExecution",
                table: "Automations");

            migrationBuilder.AddColumn<double>(
                name: "ExecutionTimeInMinutes",
                table: "Jobs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageUnSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "float",
                nullable: true);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionTimeInMinutes",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AverageSuccessfulExecutionInMinutes",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "AverageUnSuccessfulExecutionInMinutes",
                table: "Automations");

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
    }
}
