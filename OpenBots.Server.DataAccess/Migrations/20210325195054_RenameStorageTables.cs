using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RenameStorageTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttributes_ServerFiles_StorageFileId",
                table: "FileAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerFolders",
                table: "ServerFolders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerFiles",
                table: "ServerFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerDrives",
                table: "ServerDrives");

            migrationBuilder.RenameTable(
                name: "ServerFolders",
                newName: "StorageFolders");

            migrationBuilder.RenameTable(
                name: "ServerFiles",
                newName: "StorageFiles");

            migrationBuilder.RenameTable(
                name: "ServerDrives",
                newName: "StorageDrives");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StorageFolders",
                table: "StorageFolders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StorageFiles",
                table: "StorageFiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StorageDrives",
                table: "StorageDrives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttributes_StorageFiles_StorageFileId",
                table: "FileAttributes",
                column: "StorageFileId",
                principalTable: "StorageFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttributes_StorageFiles_StorageFileId",
                table: "FileAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StorageFolders",
                table: "StorageFolders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StorageFiles",
                table: "StorageFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StorageDrives",
                table: "StorageDrives");

            migrationBuilder.RenameTable(
                name: "StorageFolders",
                newName: "ServerFolders");

            migrationBuilder.RenameTable(
                name: "StorageFiles",
                newName: "ServerFiles");

            migrationBuilder.RenameTable(
                name: "StorageDrives",
                newName: "ServerDrives");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerFolders",
                table: "ServerFolders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerFiles",
                table: "ServerFiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerDrives",
                table: "ServerDrives",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttributes_ServerFiles_StorageFileId",
                table: "FileAttributes",
                column: "StorageFileId",
                principalTable: "ServerFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
