using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class seedForConfigurationValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
