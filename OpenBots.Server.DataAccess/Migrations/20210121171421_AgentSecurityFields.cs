using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AgentSecurityFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          /*  migrationBuilder.AddColumn<string>(
                name: "IPOption",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnhancedSecurity",
                table: "Agents",
                type: "bit",
                nullable: false,
                defaultValue: false);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IPOption",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "IsEnhancedSecurity",
                table: "Agents");
        }
    }
}
