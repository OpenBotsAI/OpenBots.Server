using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddDisAllowExection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisallowAllExecutions",
                table: "OrganizationSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisallowAllExecutionsMessage",
                table: "OrganizationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisallowAllExecutionsReason",
                table: "OrganizationSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisallowAllExecutions",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "DisallowAllExecutionsMessage",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "DisallowAllExecutionsReason",
                table: "OrganizationSettings");
        }
    }
}
