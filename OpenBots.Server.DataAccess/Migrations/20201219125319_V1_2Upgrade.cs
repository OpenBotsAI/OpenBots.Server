using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class V1_2Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [ConfigurationValues]", true);

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "PersonPhones");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "ProcessExecutionLogs");

            migrationBuilder.DropTable(
                name: "ProcessLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfigurationValues",
                table: "ConfigurationValues");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "App:EnableSwagger");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "App:MaxExportRecords");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "App:MaxReturnRecords");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "BinaryObjects:Adapter");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "BinaryObjects:Path");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "BinaryObjects:StorageProvider");

            migrationBuilder.DeleteData(
                table: "ConfigurationValues",
                keyColumn: "Name",
                keyValue: "Queue.Global:DefaultMaxRetryCount");

            migrationBuilder.DropColumn(
                name: "IsHealthy",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedMessage",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedOn",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedStatus",
                table: "Agents");

            migrationBuilder.DropColumn(
                name: "LastReportedWork",
                table: "Agents");

            migrationBuilder.RenameColumn(
                name: "ProcessId",
                table: "Schedules",
                newName: "QueueId");

            migrationBuilder.RenameColumn(
                name: "ProcessId",
                table: "Jobs",
                newName: "AutomationVersionId");

            migrationBuilder.AddColumn<Guid>(
                name: "AutomationId",
                table: "Schedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultJSON",
                table: "QueueItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IPFencingMode",
                table: "OrganizationSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AutomationId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "AutomationVersion",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ConfigurationValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ConfigurationValues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ConfigurationValues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "ConfigurationValues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteOn",
                table: "ConfigurationValues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ConfigurationValues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ConfigurationValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ConfigurationValues",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "ConfigurationValues",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "UIHint",
                table: "ConfigurationValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ConfigurationValues",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "ConfigurationValues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationRegex",
                table: "ConfigurationValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfigurationValues",
                table: "ConfigurationValues",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AgentHeartbeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastReportedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReportedStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastReportedWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastReportedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentHeartbeats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutomationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriggerDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasErrors = table.Column<bool>(type: "bit", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutomationLogTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AutomationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutomationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Automations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BinaryObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalPackageName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutomationEngine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutomationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    PublishedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    ContentStorageAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BinaryObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailObjectJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplyToEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntegrationEventName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccuredOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayloadJSON = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SHA256Hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PayloadSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEventSubscriptionAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventLogID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntegrationEventSubscriptionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntegrationEventName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttemptCounter = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventSubscriptionAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationEventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IntegrationEventName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransportType = table.Column<int>(type: "int", nullable: false),
                    HTTP_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HTTP_AddHeader_Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HTTP_AddHeader_Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HTTP_Max_RetryCount = table.Column<int>(type: "int", nullable: true),
                    QUEUE_QueueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IPFencings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usage = table.Column<int>(type: "int", nullable: false),
                    Rule = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeaderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeaderValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPFencings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobCheckpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Iterator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IteratorValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IteratorPosition = table.Column<int>(type: "int", nullable: true),
                    IteratorCount = table.Column<int>(type: "int", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobParameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueueItemAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BinaryObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueItemAttachments", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "IntegrationEvents",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeleteOn", "DeletedBy", "Description", "EntityType", "IsDeleted", "IsSystem", "Name", "PayloadSchema", "UpdatedBy", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("744ba6f9-161f-41dc-b76e-c1602fc65d1b"), "", null, null, "", "A Queue has been updated", "Queue", false, true, "Queues.QueueUpdated", null, null, null },
                    { new Guid("6ce8b3da-0373-4da2-bc77-ea845212855d"), "", null, null, "", "A new agent has been created", "Agent", false, true, "Agents.NewAgentCreated", null, null, null },
                    { new Guid("35fd2aa3-6c77-4995-9ed8-9b262e5afdfc"), "", null, null, "", "An Agent has reported an unhealthy status", "Agent", false, true, "Agents.UnhealthyReported", null, null, null },
                    { new Guid("6e0c741c-34b0-471e-a491-c7ec61782e94"), "", null, null, "", "An Asset has been deleted", "Asset", false, true, "Assets.AssetDeleted", null, null, null },
                    { new Guid("4ce67735-2edc-4b7f-849a-5575740a496f"), "", null, null, "", "An Asset has been updated", "Asset", false, true, "Assets.AssetUpdated", null, null, null },
                    { new Guid("f1b111cc-1f26-404d-827c-e30305c2ecc4"), "", null, null, "", "A new Asset has been created", "Asset", false, true, "Assets.NewAssetCreated", null, null, null },
                    { new Guid("90f9f691-90e5-41d0-9b2c-1e8437bc85d3"), "", null, null, "", "A Process has been deleted", "Automation", false, true, "Automations.AutomationDeleted", null, null, null },
                    { new Guid("8437fa1f-777a-4905-a169-feb32214c0c8"), "", null, null, "", "A Process has been updated", "Automation", false, true, "Automations.AutomationUpdated", null, null, null },
                    { new Guid("93416738-3284-4bb0-869e-e2f191446b44"), "", null, null, "", "A new Process has been created", "Automation", false, true, "Automations.NewAutomationCreated", null, null, null },
                    { new Guid("ecced501-9c35-4b37-a7b2-b6b901f91234"), "", null, null, "", "A Credential has been deleted", "Credential", false, true, "Credentials.CredentialDeleted", null, null, null },
                    { new Guid("efd1d688-1881-4d5e-aed7-81528d54d7ef"), "", null, null, "", "A Credential has been updated", "Credential", false, true, "Credentials.CredentialUpdated", null, null, null },
                    { new Guid("2b4bd195-62ac-4111-97ca-d6df6dd3f0fb"), "", null, null, "", "An Agent has been updated", "Agent", false, true, "Agents.AgentUpdated", null, null, null },
                    { new Guid("76f6ab13-c430-46ad-b859-3d2dfd802e84"), "", null, null, "", "A new Credential has been created", "Credential", false, true, "Credentials.NewCredentialCreated", null, null, null },
                    { new Guid("3ff9b456-7832-4499-b263-692c021e7d80"), "", null, null, "", "A File has been updated", "File", false, true, "Files.FileUpdated", null, null, null },
                    { new Guid("04cf6a7a-ca72-48bc-887f-666ef580d198"), "", null, null, "", "A new File has been created", "File", false, true, "Files.NewFileCreated", null, null, null },
                    { new Guid("82b8d08d-5ae2-4031-bdf8-5fba5597ac4b"), "", null, null, "", "A Job has been deleted", "Job", false, true, "Jobs.JobsDeleted", null, null, null },
                    { new Guid("9d8e576a-a69d-43cf-bbc9-18103105d0a0"), "", null, null, "", "A Job has been updated", "Job", false, true, "Jobs.JobUpdated", null, null, null },
                    { new Guid("06dd9940-a483-4a21-9551-cf2e32eeccae"), "", null, null, "", "A new Job has been created", "Job", false, true, "Jobs.NewJobCreated", null, null, null },
                    { new Guid("30a8dcb9-10cf-43c6-a08f-b45fe2125dae"), "", null, null, "", "A new QueueItem has been created", "QueueItem", false, true, "QueueItems.NewQueueItemCreated", null, null, null },
                    { new Guid("860689af-fd19-44ba-a5c7-53f6fed92065"), "", null, null, "", "A QueueItem has been deleted", "QueueItem", false, true, "QueueItems.QueueItemDeleted", null, null, null },
                    { new Guid("0719a4c3-2143-4b9a-92ae-8b5a93075b98"), "", null, null, "", "A QueueItem has been updated", "QueueItem", false, true, "QueueItems.QueueItemUpdated", null, null, null },
                    { new Guid("e9f64119-edbf-4779-a796-21ad59f76534"), "", null, null, "", "A new Queue has been created", "Queue", false, true, "Queues.NewQueueCreated", null, null, null },
                    { new Guid("b00eeecd-5729-4f82-9cd2-dcfafd946965"), "", null, null, "", "A Queue has been deleted", "Queue", false, true, "Queues.QueueDeleted", null, null, null },
                    { new Guid("32d63e9d-aa6e-481f-b928-541ddf979bdf"), "", null, null, "", "A File has been deleted", "File", false, true, "Files.FileDeleted", null, null, null },
                    { new Guid("6ce0bb0e-cda1-49fa-a9e4-b67d904f826e"), "", null, null, "", "An Agent has been deleted", "Agent", false, true, "Agents.AgentDeleted", null, null, null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentHeartbeats");

            migrationBuilder.DropTable(
                name: "AutomationExecutionLogs");

            migrationBuilder.DropTable(
                name: "AutomationLogs");

            migrationBuilder.DropTable(
                name: "Automations");

            migrationBuilder.DropTable(
                name: "AutomationVersions");

            migrationBuilder.DropTable(
                name: "EmailAttachments");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "IntegrationEventLogs");

            migrationBuilder.DropTable(
                name: "IntegrationEvents");

            migrationBuilder.DropTable(
                name: "IntegrationEventSubscriptionAttempts");

            migrationBuilder.DropTable(
                name: "IntegrationEventSubscriptions");

            migrationBuilder.DropTable(
                name: "IPFencings");

            migrationBuilder.DropTable(
                name: "JobCheckpoints");

            migrationBuilder.DropTable(
                name: "JobParameters");

            migrationBuilder.DropTable(
                name: "QueueItemAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfigurationValues",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "AutomationId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ResultJSON",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "IPFencingMode",
                table: "OrganizationSettings");

            migrationBuilder.DropColumn(
                name: "AutomationId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AutomationVersion",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "DeleteOn",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "UIHint",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "ConfigurationValues");

            migrationBuilder.DropColumn(
                name: "ValidationRegex",
                table: "ConfigurationValues");

            migrationBuilder.RenameColumn(
                name: "QueueId",
                table: "Schedules",
                newName: "ProcessId");

            migrationBuilder.RenameColumn(
                name: "AutomationVersionId",
                table: "Jobs",
                newName: "ProcessId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ConfigurationValues",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BinaryObjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsHealthy",
                table: "Agents",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedMessage",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReportedOn",
                table: "Agents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedStatus",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReportedWork",
                table: "Agents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfigurationValues",
                table: "ConfigurationValues",
                column: "Name");

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailObjectJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SentOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getutcdate()"),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(99)", maxLength: 99, nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonPhones_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BinaryObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasErrors = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    JobID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TriggerDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessLogTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonPhones_PersonId",
                table: "PersonPhones",
                column: "PersonId");
        }
    }
}
