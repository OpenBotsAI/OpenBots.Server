using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class UpdateQueueItemAndAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.AddColumn<long>(
                name: "PayloadSizeInBytes",
                table: "QueueItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SizeInBytes",
                table: "QueueItemAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);*/
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayloadSizeInBytes",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "SizeInBytes",
                table: "QueueItemAttachments");
        }
    }
}
