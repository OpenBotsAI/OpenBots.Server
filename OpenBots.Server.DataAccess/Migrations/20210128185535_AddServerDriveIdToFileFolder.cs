using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class AddServerDriveIdToFileFolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.AddColumn<Guid>(
                name: "ServerDriveId",
                table: "ServerFolders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServerDriveId",
                table: "ServerFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "ServerDrives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServerDriveId",
                table: "FileAttributes",
                type: "uniqueidentifier",
                nullable: true);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerDriveId",
                table: "ServerFolders");

            migrationBuilder.DropColumn(
                name: "ServerDriveId",
                table: "ServerFiles");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "ServerDrives");

            migrationBuilder.DropColumn(
                name: "ServerDriveId",
                table: "FileAttributes");
        }
    }
}
