using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class UpdateEmailLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailAttachments_EmailLogs_EmailLogId",
                table: "EmailAttachments");

            migrationBuilder.DropIndex(
                name: "IX_EmailAttachments_EmailLogId",
                table: "EmailAttachments");

            migrationBuilder.AddColumn<string>(
                name: "EmailAttachments",
                table: "EmailLogs",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmailLogId",
                table: "EmailAttachments",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAttachments",
                table: "EmailLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "EmailLogId",
                table: "EmailAttachments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.CreateIndex(
                name: "IX_EmailAttachments_EmailLogId",
                table: "EmailAttachments",
                column: "EmailLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailAttachments_EmailLogs_EmailLogId",
                table: "EmailAttachments",
                column: "EmailLogId",
                principalTable: "EmailLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
