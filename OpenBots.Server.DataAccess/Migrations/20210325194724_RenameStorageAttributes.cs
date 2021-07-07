using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RenameStorageAttributes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttributes_ServerFiles_ServerFileId",
                table: "FileAttributes");

            migrationBuilder.RenameColumn(
                name: "ServerDriveId",
                table: "ServerFiles",
                newName: "StorageDriveId");

            migrationBuilder.RenameColumn(
                name: "ServerFileId",
                table: "FileAttributes",
                newName: "StorageFileId");

            migrationBuilder.RenameColumn(
                name: "ServerDriveId",
                table: "FileAttributes",
                newName: "StorageDriveId");

            migrationBuilder.RenameIndex(
                name: "IX_FileAttributes_ServerFileId",
                table: "FileAttributes",
                newName: "IX_FileAttributes_StorageFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttributes_ServerFiles_StorageFileId",
                table: "FileAttributes",
                column: "StorageFileId",
                principalTable: "ServerFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileAttributes_ServerFiles_StorageFileId",
                table: "FileAttributes");

            migrationBuilder.RenameColumn(
                name: "StorageDriveId",
                table: "ServerFiles",
                newName: "ServerDriveId");

            migrationBuilder.RenameColumn(
                name: "StorageFileId",
                table: "FileAttributes",
                newName: "ServerFileId");

            migrationBuilder.RenameColumn(
                name: "StorageDriveId",
                table: "FileAttributes",
                newName: "ServerDriveId");

            migrationBuilder.RenameIndex(
                name: "IX_FileAttributes_StorageFileId",
                table: "FileAttributes",
                newName: "IX_FileAttributes_ServerFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileAttributes_ServerFiles_ServerFileId",
                table: "FileAttributes",
                column: "ServerFileId",
                principalTable: "ServerFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
