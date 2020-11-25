using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class UpdateProcessVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessVersions_Processes_ProcessId",
                table: "ProcessVersions");

            migrationBuilder.DropIndex(
                name: "IX_ProcessVersions_ProcessId",
                table: "ProcessVersions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProcessVersions_ProcessId",
                table: "ProcessVersions",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessVersions_Processes_ProcessId",
                table: "ProcessVersions",
                column: "ProcessId",
                principalTable: "Processes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
