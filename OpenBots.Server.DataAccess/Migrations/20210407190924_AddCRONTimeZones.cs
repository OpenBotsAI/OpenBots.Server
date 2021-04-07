using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddCRONTimeZones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CRONExpressionTimeZone",
                table: "Schedules",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CRONExpressionTimeZone",
                table: "Schedules");
        }
    }
}
