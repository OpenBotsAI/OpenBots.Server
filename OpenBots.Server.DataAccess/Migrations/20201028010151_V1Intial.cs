using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenBots.Server.DataAccess.Migrations
{
    public partial class V1Intial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    MachineName = table.Column<string>(nullable: false),
                    MacAddresses = table.Column<string>(nullable: true),
                    IPAddresses = table.Column<string>(nullable: true),
                    IsEnabled = table.Column<bool>(nullable: false),
                    LastReportedOn = table.Column<DateTime>(nullable: true),
                    LastReportedStatus = table.Column<string>(nullable: true),
                    LastReportedWork = table.Column<string>(nullable: true),
                    LastReportedMessage = table.Column<string>(nullable: true),
                    IsHealthy = table.Column<bool>(nullable: true),
                    IsConnected = table.Column<bool>(nullable: false),
                    CredentialId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppVersion",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Major = table.Column<int>(nullable: false),
                    Minor = table.Column<int>(nullable: false),
                    Patch = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersion", x => x.Id);
                });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        UserName = table.Column<string>(nullable: true),
            //        NormalizedUserName = table.Column<string>(nullable: true),
            //        Email = table.Column<string>(nullable: true),
            //        NormalizedEmail = table.Column<string>(nullable: true),
            //        EmailConfirmed = table.Column<bool>(nullable: false),
            //        PhoneNumber = table.Column<string>(nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(nullable: false),
            //        PersonId = table.Column<Guid>(nullable: false),
            //        ForcedPasswordChange = table.Column<bool>(nullable: false),
            //        LockoutEnabled = table.Column<bool>(nullable: false),
            //        AccessFailedCount = table.Column<int>(nullable: false),
            //        IsUserConsentRequired = table.Column<bool>(nullable: true),
            //        Name = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Type = table.Column<string>(nullable: false),
                    TextValue = table.Column<string>(nullable: true),
                    NumberValue = table.Column<double>(nullable: true),
                    JsonValue = table.Column<string>(nullable: true),
                    BinaryObjectID = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    ObjectId = table.Column<Guid>(nullable: true),
                    ServiceName = table.Column<string>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    ParametersJson = table.Column<string>(nullable: true),
                    ExceptionJson = table.Column<string>(nullable: true),
                    ChangedFromJson = table.Column<string>(nullable: true),
                    ChangedToJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BinaryObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    CorrelationEntityId = table.Column<Guid>(nullable: true),
                    CorrelationEntity = table.Column<string>(nullable: true),
                    Folder = table.Column<string>(nullable: true),
                    StoragePath = table.Column<string>(nullable: true),
                    StorageProvider = table.Column<string>(nullable: true),
                    SizeInBytes = table.Column<long>(nullable: true),
                    HashCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinaryObjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationValues",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationValues", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Provider = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Domain = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(nullable: false),
                    PasswordSecret = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    Certificate = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    IsDisabled = table.Column<bool>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false),
                    Provider = table.Column<string>(nullable: true),
                    IsSslEnabled = table.Column<bool>(nullable: false),
                    Host = table.Column<string>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    EncryptedPassword = table.Column<string>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    ApiKey = table.Column<string>(nullable: true),
                    FromEmailAddress = table.Column<string>(nullable: true),
                    FromName = table.Column<string>(nullable: true),
                    StartOnUTC = table.Column<DateTime>(nullable: false),
                    EndOnUTC = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    EmailAccountId = table.Column<Guid>(nullable: false),
                    SentOnUTC = table.Column<DateTime>(nullable: false),
                    EmailObjectJson = table.Column<string>(nullable: true),
                    SenderName = table.Column<string>(nullable: true),
                    SenderAddress = table.Column<string>(nullable: true),
                    SenderUserId = table.Column<Guid>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    IsEmailDisabled = table.Column<bool>(nullable: false),
                    AddToAddress = table.Column<string>(nullable: true),
                    AddCCAddress = table.Column<string>(nullable: true),
                    AddBCCAddress = table.Column<string>(nullable: true),
                    AllowedDomains = table.Column<string>(nullable: true),
                    BlockedDomains = table.Column<string>(nullable: true),
                    SubjectAddPrefix = table.Column<string>(nullable: true),
                    SubjectAddSuffix = table.Column<string>(nullable: true),
                    BodyAddPrefix = table.Column<string>(nullable: true),
                    BodyAddSuffix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    AgentId = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: true),
                    EndTime = table.Column<DateTime>(nullable: true),
                    EnqueueTime = table.Column<DateTime>(nullable: true),
                    DequeueTime = table.Column<DateTime>(nullable: true),
                    ProcessId = table.Column<Guid>(nullable: false),
                    JobStatus = table.Column<int>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IsSuccessful = table.Column<bool>(nullable: true),
                    ErrorReason = table.Column<string>(nullable: true),
                    ErrorCode = table.Column<string>(nullable: true),
                    SerializedErrorString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    LookupCode = table.Column<string>(nullable: false),
                    LookupDesc = table.Column<string>(nullable: true),
                    CodeType = table.Column<string>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: true),
                    SequenceOrder = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    MinimumLength = table.Column<int>(nullable: true),
                    RequireAtleastOneUppercase = table.Column<bool>(nullable: true),
                    RequireAtleastOneLowercase = table.Column<bool>(nullable: true),
                    RequireAtleastOneNonAlpha = table.Column<bool>(nullable: true),
                    RequireAtleastOneNumber = table.Column<bool>(nullable: true),
                    EnableExpiration = table.Column<bool>(nullable: true),
                    ExpiresInDays = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    IsAgent = table.Column<bool>(nullable: false),
                    Company = table.Column<string>(nullable: true),
                    Department = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Version = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    BinaryObjectId = table.Column<Guid>(nullable: false),
                    VersionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    JobID = table.Column<Guid>(nullable: false),
                    ProcessID = table.Column<Guid>(nullable: false),
                    AgentID = table.Column<Guid>(nullable: false),
                    StartedOn = table.Column<DateTime>(nullable: false),
                    CompletedOn = table.Column<DateTime>(nullable: false),
                    Trigger = table.Column<string>(nullable: true),
                    TriggerDetails = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    HasErrors = table.Column<bool>(nullable: true),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorDetails = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    MessageTemplate = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    ProcessLogTimeStamp = table.Column<DateTime>(nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    JobId = table.Column<Guid>(nullable: true),
                    ProcessId = table.Column<Guid>(nullable: true),
                    AgentId = table.Column<Guid>(nullable: true),
                    MachineName = table.Column<string>(nullable: true),
                    AgentName = table.Column<string>(nullable: true),
                    ProcessName = table.Column<string>(nullable: true),
                    Logger = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueueItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    IsLocked = table.Column<bool>(nullable: false),
                    LockedOnUTC = table.Column<DateTime>(nullable: true),
                    LockedUntilUTC = table.Column<DateTime>(nullable: true),
                    LockedBy = table.Column<Guid>(nullable: true),
                    QueueId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    JsonType = table.Column<string>(nullable: true),
                    DataJson = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    StateMessage = table.Column<string>(nullable: true),
                    LockTransactionKey = table.Column<Guid>(nullable: true),
                    LockedEndTimeUTC = table.Column<DateTime>(nullable: true),
                    RetryCount = table.Column<int>(nullable: false),
                    Priority = table.Column<int>(nullable: false),
                    ExpireOnUTC = table.Column<DateTime>(nullable: true),
                    PostponeUntilUTC = table.Column<DateTime>(nullable: true),
                    ErrorCode = table.Column<string>(nullable: true),
                    ErrorMessage = table.Column<string>(nullable: true),
                    ErrorSerialized = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Queues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    MaxRetryCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    AgentId = table.Column<Guid>(nullable: true),
                    AgentName = table.Column<string>(nullable: true),
                    CRONExpression = table.Column<string>(nullable: true),
                    LastExecution = table.Column<DateTime>(nullable: true),
                    NextExecution = table.Column<DateTime>(nullable: true),
                    IsDisabled = table.Column<bool>(nullable: true),
                    ProjectId = table.Column<Guid>(nullable: true),
                    TriggerName = table.Column<string>(nullable: true),
                    Recurrence = table.Column<bool>(nullable: true),
                    StartingType = table.Column<string>(nullable: true),
                    StartJobOn = table.Column<DateTime>(nullable: true),
                    RecurrenceUnit = table.Column<DateTime>(nullable: true),
                    JobRecurEveryUnit = table.Column<DateTime>(nullable: true),
                    EndJobOn = table.Column<DateTime>(nullable: true),
                    EndJobAtOccurence = table.Column<DateTime>(nullable: true),
                    NoJobEndDate = table.Column<DateTime>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    ProcessId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    Version = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    ContentStaticUrl = table.Column<string>(nullable: true),
                    EffectiveOnUTC = table.Column<DateTime>(nullable: true),
                    ExpiresOnUTC = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAgreements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: true),
                    TimeZone = table.Column<string>(nullable: true),
                    StorageLocation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    IsVisibleToAllOrganizationMembers = table.Column<bool>(nullable: true),
                    CanDelete = table.Column<bool>(nullable: true, defaultValueSql: "1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    IsAccessRequested = table.Column<bool>(nullable: true),
                    AccessRequestedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessRequests_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(maxLength: 256, nullable: false),
                    IsVerified = table.Column<bool>(nullable: true),
                    VerificationEmailCount = table.Column<int>(nullable: false),
                    VerificationCode = table.Column<string>(maxLength: 100, nullable: true),
                    VerificationCodeExpiresOn = table.Column<DateTime>(nullable: true),
                    IsVerificationEmailSent = table.Column<bool>(nullable: true),
                    VerificationSentOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerifications_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    IsAdministrator = table.Column<bool>(nullable: true),
                    ApprovedBy = table.Column<string>(nullable: true),
                    ApprovedOn = table.Column<DateTime>(nullable: true),
                    IsInvited = table.Column<bool>(nullable: true),
                    InvitedBy = table.Column<string>(nullable: true),
                    InvitedOn = table.Column<DateTime>(nullable: true),
                    InviteAccepted = table.Column<bool>(nullable: true),
                    InviteAcceptedOn = table.Column<DateTime>(nullable: true),
                    IsAutoApprovedByEmailAddress = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationMembers_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    Secret = table.Column<string>(nullable: true),
                    Salt = table.Column<string>(nullable: true),
                    IsExpired = table.Column<bool>(nullable: true),
                    ExpiresOnUTC = table.Column<DateTime>(nullable: true),
                    ForceChange = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonCredentials_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonEmails",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(maxLength: 256, nullable: false),
                    EmailVerificationId = table.Column<Guid>(nullable: false),
                    IsPrimaryEmail = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonEmails_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    PhoneNumber = table.Column<string>(maxLength: 99, nullable: true)
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
                name: "UserConsents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    PersonId = table.Column<Guid>(nullable: false),
                    UserAgreementID = table.Column<Guid>(nullable: false),
                    IsAccepted = table.Column<bool>(nullable: false),
                    RecordedOn = table.Column<DateTime>(nullable: true),
                    ExpiresOnUTC = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserConsents_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserConsents_UserAgreements_UserAgreementID",
                        column: x => x.UserAgreementID,
                        principalTable: "UserAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    IsDeleted = table.Column<bool>(nullable: true, defaultValue: false),
                    CreatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true, defaultValueSql: "getutcdate()"),
                    DeletedBy = table.Column<string>(maxLength: 100, nullable: true),
                    DeleteOn = table.Column<DateTime>(nullable: true),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 100, nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationUnitId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    IsAdministrator = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnitMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitMembers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitMembers_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitMembers_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_OrganizationId",
                table: "AccessRequests",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_PersonId",
                table: "AccessRequests",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_PersonId",
                table: "EmailVerifications",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_OrganizationId",
                table: "OrganizationMembers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationMembers_PersonId",
                table: "OrganizationMembers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId",
                table: "OrganizationSettings",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitMembers_OrganizationId",
                table: "OrganizationUnitMembers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitMembers_OrganizationUnitId",
                table: "OrganizationUnitMembers",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitMembers_PersonId",
                table: "OrganizationUnitMembers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_OrganizationId_Name",
                table: "OrganizationUnits",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonCredentials_PersonId",
                table: "PersonCredentials",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonEmails_PersonId",
                table: "PersonEmails",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPhones_PersonId",
                table: "PersonPhones",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_PersonId",
                table: "UserConsents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserAgreementID",
                table: "UserConsents",
                column: "UserAgreementID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessRequests");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "AppVersion");

            //migrationBuilder.DropTable(
            //    name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BinaryObjects");

            migrationBuilder.DropTable(
                name: "ConfigurationValues");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "EmailAccounts");

            migrationBuilder.DropTable(
                name: "EmailLogs");

            migrationBuilder.DropTable(
                name: "EmailSettings");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "LookupValues");

            migrationBuilder.DropTable(
                name: "OrganizationMembers");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.DropTable(
                name: "OrganizationUnitMembers");

            migrationBuilder.DropTable(
                name: "PasswordPolicies");

            migrationBuilder.DropTable(
                name: "PersonCredentials");

            migrationBuilder.DropTable(
                name: "PersonEmails");

            migrationBuilder.DropTable(
                name: "PersonPhones");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "ProcessExecutionLogs");

            migrationBuilder.DropTable(
                name: "ProcessLogs");

            migrationBuilder.DropTable(
                name: "QueueItems");

            migrationBuilder.DropTable(
                name: "Queues");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "UserConsents");

            migrationBuilder.DropTable(
                name: "OrganizationUnits");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "UserAgreements");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
