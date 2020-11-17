using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class RemoveConfigurationValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationValues");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationValues",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationValues", x => x.Name);
                });

            migrationBuilder.InsertData(
                table: "ConfigurationValues",
                columns: new[] { "Name", "Value" },
                values: new object[,]
                {
                    { "BinaryObjects:Adapter", "FileSystemAdapter" },
                    { "BinaryObjects:Path", "BinaryObjects" },
                    { "BinaryObjects:StorageProvider", "FileSystem.Default" },
                    { "Queue.Global:DefaultMaxRetryCount", "2" },
                    { "App:MaxExportRecords", "100" },
                    { "App:MaxReturnRecords", "100" },
                    { "App:EnableSwagger", "true" }
                });
        }
    }
}
