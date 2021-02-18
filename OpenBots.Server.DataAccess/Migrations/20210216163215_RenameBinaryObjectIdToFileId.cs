using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RenameBinaryObjectIdToFileId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BinaryObjectId",
                table: "Automations",
                newName: "FileId");

            migrationBuilder.RenameColumn(
                name: "BinaryObjectId",
                table: "QueueItemAttachments",
                newName: "FileId");

            migrationBuilder.RenameColumn(
                name: "BinaryObjectId",
                table: "EmailAttachments",
                newName: "FileId");

            migrationBuilder.RenameColumn(
                name: "BinaryObjectID",
                table: "Assets",
                newName: "FileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Automations",
                newName: "BinaryObjectId");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "QueueItemAttachments",
                newName: "BinaryObjectId");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "EmailAttachments",
                newName: "BinaryObjectId");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Assets",
                newName: "BinaryObjectID");
        }
    }
}
