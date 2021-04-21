using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class UseLongTypeForMinutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ExecutionTimeInMinutes",
                table: "Jobs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AverageUnSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AverageSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ExecutionTimeInMinutes",
                table: "Jobs",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AverageUnSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AverageSuccessfulExecutionInMinutes",
                table: "Automations",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
