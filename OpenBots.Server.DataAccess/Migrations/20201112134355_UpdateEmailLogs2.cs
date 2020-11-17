using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class UpdateEmailLogs2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAttachments",
                table: "EmailLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAttachments",
                table: "EmailLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
