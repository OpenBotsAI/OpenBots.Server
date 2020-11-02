IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Agents] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [MachineName] nvarchar(max) NOT NULL,
        [MacAddresses] nvarchar(max) NULL,
        [IPAddresses] nvarchar(max) NULL,
        [IsEnabled] bit NOT NULL,
        [LastReportedOn] datetime2 NULL,
        [LastReportedStatus] nvarchar(max) NULL,
        [LastReportedWork] nvarchar(max) NULL,
        [LastReportedMessage] nvarchar(max) NULL,
        [IsHealthy] bit NULL,
        [IsConnected] bit NOT NULL,
        [CredentialId] uniqueidentifier NULL,
        CONSTRAINT [PK_Agents] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [AppVersion] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Major] int NOT NULL,
        [Minor] int NOT NULL,
        [Patch] int NOT NULL,
        CONSTRAINT [PK_AppVersion] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Assets] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [TextValue] nvarchar(max) NULL,
        [NumberValue] float NULL,
        [JsonValue] nvarchar(max) NULL,
        [BinaryObjectID] uniqueidentifier NULL,
        CONSTRAINT [PK_Assets] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [ObjectId] uniqueidentifier NULL,
        [ServiceName] nvarchar(max) NULL,
        [MethodName] nvarchar(max) NULL,
        [ParametersJson] nvarchar(max) NULL,
        [ExceptionJson] nvarchar(max) NULL,
        [ChangedFromJson] nvarchar(max) NULL,
        [ChangedToJson] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [BinaryObjects] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [OrganizationId] uniqueidentifier NULL,
        [ContentType] nvarchar(max) NULL,
        [CorrelationEntityId] uniqueidentifier NULL,
        [CorrelationEntity] nvarchar(max) NULL,
        [Folder] nvarchar(max) NULL,
        [StoragePath] nvarchar(max) NULL,
        [StorageProvider] nvarchar(max) NULL,
        [SizeInBytes] bigint NULL,
        [HashCode] nvarchar(max) NULL,
        CONSTRAINT [PK_BinaryObjects] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [ConfigurationValues] (
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_ConfigurationValues] PRIMARY KEY ([Name])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Credentials] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Provider] nvarchar(max) NULL,
        [StartDate] datetime2 NULL,
        [EndDate] datetime2 NULL,
        [Domain] nvarchar(max) NULL,
        [UserName] nvarchar(max) NOT NULL,
        [PasswordSecret] nvarchar(max) NULL,
        [PasswordHash] nvarchar(max) NULL,
        [Certificate] nvarchar(max) NULL,
        CONSTRAINT [PK_Credentials] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [EmailAccounts] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsDisabled] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [Provider] nvarchar(max) NULL,
        [IsSslEnabled] bit NOT NULL,
        [Host] nvarchar(max) NULL,
        [Port] int NOT NULL,
        [Username] nvarchar(max) NULL,
        [EncryptedPassword] nvarchar(max) NULL,
        [PasswordHash] nvarchar(max) NULL,
        [ApiKey] nvarchar(max) NULL,
        [FromEmailAddress] nvarchar(max) NULL,
        [FromName] nvarchar(max) NULL,
        [StartOnUTC] datetime2 NOT NULL,
        [EndOnUTC] datetime2 NOT NULL,
        CONSTRAINT [PK_EmailAccounts] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [EmailLogs] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [EmailAccountId] uniqueidentifier NOT NULL,
        [SentOnUTC] datetime2 NOT NULL,
        [EmailObjectJson] nvarchar(max) NULL,
        [SenderName] nvarchar(max) NULL,
        [SenderAddress] nvarchar(max) NULL,
        [SenderUserId] uniqueidentifier NULL,
        [Status] nvarchar(max) NULL,
        [Reason] nvarchar(max) NULL,
        CONSTRAINT [PK_EmailLogs] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [EmailSettings] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [IsEmailDisabled] bit NOT NULL,
        [AddToAddress] nvarchar(max) NULL,
        [AddCCAddress] nvarchar(max) NULL,
        [AddBCCAddress] nvarchar(max) NULL,
        [AllowedDomains] nvarchar(max) NULL,
        [BlockedDomains] nvarchar(max) NULL,
        [SubjectAddPrefix] nvarchar(max) NULL,
        [SubjectAddSuffix] nvarchar(max) NULL,
        [BodyAddPrefix] nvarchar(max) NULL,
        [BodyAddSuffix] nvarchar(max) NULL,
        CONSTRAINT [PK_EmailSettings] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Jobs] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [AgentId] uniqueidentifier NOT NULL,
        [StartTime] datetime2 NULL,
        [EndTime] datetime2 NULL,
        [EnqueueTime] datetime2 NULL,
        [DequeueTime] datetime2 NULL,
        [ProcessId] uniqueidentifier NOT NULL,
        [JobStatus] int NULL,
        [Message] nvarchar(max) NULL,
        [IsSuccessful] bit NULL,
        [ErrorReason] nvarchar(max) NULL,
        [ErrorCode] nvarchar(max) NULL,
        [SerializedErrorString] nvarchar(max) NULL,
        CONSTRAINT [PK_Jobs] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [LookupValues] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [LookupCode] nvarchar(max) NOT NULL,
        [LookupDesc] nvarchar(max) NULL,
        [CodeType] nvarchar(max) NOT NULL,
        [OrganizationId] uniqueidentifier NULL,
        [SequenceOrder] int NULL,
        CONSTRAINT [PK_LookupValues] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Organizations] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [PasswordPolicies] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [MinimumLength] int NULL,
        [RequireAtleastOneUppercase] bit NULL,
        [RequireAtleastOneLowercase] bit NULL,
        [RequireAtleastOneNonAlpha] bit NULL,
        [RequireAtleastOneNumber] bit NULL,
        [EnableExpiration] bit NULL,
        [ExpiresInDays] int NULL,
        CONSTRAINT [PK_PasswordPolicies] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [People] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [IsAgent] bit NOT NULL,
        [Company] nvarchar(max) NULL,
        [Department] nvarchar(max) NULL,
        CONSTRAINT [PK_People] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Processes] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Version] int NOT NULL,
        [Status] nvarchar(max) NULL,
        [BinaryObjectId] uniqueidentifier NOT NULL,
        [VersionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Processes] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [ProcessExecutionLogs] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [JobID] uniqueidentifier NOT NULL,
        [ProcessID] uniqueidentifier NOT NULL,
        [AgentID] uniqueidentifier NOT NULL,
        [StartedOn] datetime2 NOT NULL,
        [CompletedOn] datetime2 NOT NULL,
        [Trigger] nvarchar(max) NULL,
        [TriggerDetails] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [HasErrors] bit NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ErrorDetails] nvarchar(max) NULL,
        CONSTRAINT [PK_ProcessExecutionLogs] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [ProcessLogs] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Message] nvarchar(max) NULL,
        [MessageTemplate] nvarchar(max) NULL,
        [Level] nvarchar(max) NULL,
        [ProcessLogTimeStamp] datetime2 NULL,
        [Exception] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [JobId] uniqueidentifier NULL,
        [ProcessId] uniqueidentifier NULL,
        [AgentId] uniqueidentifier NULL,
        [MachineName] nvarchar(max) NULL,
        [AgentName] nvarchar(max) NULL,
        [ProcessName] nvarchar(max) NULL,
        [Logger] nvarchar(max) NULL,
        CONSTRAINT [PK_ProcessLogs] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [QueueItems] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsLocked] bit NOT NULL,
        [LockedOnUTC] datetime2 NULL,
        [LockedUntilUTC] datetime2 NULL,
        [LockedBy] uniqueidentifier NULL,
        [QueueId] uniqueidentifier NOT NULL,
        [Type] nvarchar(max) NULL,
        [JsonType] nvarchar(max) NULL,
        [DataJson] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [StateMessage] nvarchar(max) NULL,
        [LockTransactionKey] uniqueidentifier NULL,
        [LockedEndTimeUTC] datetime2 NULL,
        [RetryCount] int NOT NULL,
        [Priority] int NOT NULL,
        [ExpireOnUTC] datetime2 NULL,
        [PostponeUntilUTC] datetime2 NULL,
        [ErrorCode] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ErrorSerialized] nvarchar(max) NULL,
        [Source] nvarchar(max) NULL,
        [Event] nvarchar(max) NULL,
        CONSTRAINT [PK_QueueItems] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Queues] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [MaxRetryCount] int NOT NULL,
        CONSTRAINT [PK_Queues] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [Schedules] (
        [Id] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        [AgentId] uniqueidentifier NULL,
        [AgentName] nvarchar(max) NULL,
        [CRONExpression] nvarchar(max) NULL,
        [LastExecution] datetime2 NULL,
        [NextExecution] datetime2 NULL,
        [IsDisabled] bit NULL,
        [ProjectId] uniqueidentifier NULL,
        [TriggerName] nvarchar(max) NULL,
        [Recurrence] bit NULL,
        [StartingType] nvarchar(max) NULL,
        [StartJobOn] datetime2 NULL,
        [RecurrenceUnit] datetime2 NULL,
        [JobRecurEveryUnit] datetime2 NULL,
        [EndJobOn] datetime2 NULL,
        [EndJobAtOccurence] datetime2 NULL,
        [NoJobEndDate] datetime2 NULL,
        [Status] nvarchar(max) NULL,
        [ExpiryDate] datetime2 NULL,
        [StartDate] datetime2 NULL,
        [ProcessId] uniqueidentifier NULL,
        CONSTRAINT [PK_Schedules] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [UserAgreements] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Version] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [ContentStaticUrl] nvarchar(max) NULL,
        [EffectiveOnUTC] datetime2 NULL,
        [ExpiresOnUTC] datetime2 NULL,
        CONSTRAINT [PK_UserAgreements] PRIMARY KEY ([Id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [OrganizationSettings] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NULL,
        [TimeZone] nvarchar(max) NULL,
        [StorageLocation] nvarchar(max) NULL,
        CONSTRAINT [PK_OrganizationSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationSettings_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [OrganizationUnits] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsVisibleToAllOrganizationMembers] bit NULL,
        [CanDelete] bit NULL DEFAULT (1),
        CONSTRAINT [PK_OrganizationUnits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationUnits_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [AccessRequests] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [IsAccessRequested] bit NULL,
        [AccessRequestedOn] datetime2 NULL,
        CONSTRAINT [PK_AccessRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AccessRequests_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccessRequests_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [EmailVerifications] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [Address] nvarchar(256) NOT NULL,
        [IsVerified] bit NULL,
        [VerificationEmailCount] int NOT NULL,
        [VerificationCode] nvarchar(100) NULL,
        [VerificationCodeExpiresOn] datetime2 NULL,
        [IsVerificationEmailSent] bit NULL,
        [VerificationSentOn] datetime2 NULL,
        CONSTRAINT [PK_EmailVerifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailVerifications_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [OrganizationMembers] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [IsAdministrator] bit NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedOn] datetime2 NULL,
        [IsInvited] bit NULL,
        [InvitedBy] nvarchar(max) NULL,
        [InvitedOn] datetime2 NULL,
        [InviteAccepted] bit NULL,
        [InviteAcceptedOn] datetime2 NULL,
        [IsAutoApprovedByEmailAddress] bit NULL,
        CONSTRAINT [PK_OrganizationMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationMembers_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrganizationMembers_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [PersonCredentials] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [Secret] nvarchar(max) NULL,
        [Salt] nvarchar(max) NULL,
        [IsExpired] bit NULL,
        [ExpiresOnUTC] datetime2 NULL,
        [ForceChange] bit NULL,
        CONSTRAINT [PK_PersonCredentials] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PersonCredentials_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [PersonEmails] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [Address] nvarchar(256) NOT NULL,
        [EmailVerificationId] uniqueidentifier NOT NULL,
        [IsPrimaryEmail] bit NOT NULL,
        CONSTRAINT [PK_PersonEmails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PersonEmails_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [PersonPhones] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [PhoneNumber] nvarchar(99) NULL,
        CONSTRAINT [PK_PersonPhones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PersonPhones_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [UserConsents] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [UserAgreementID] uniqueidentifier NOT NULL,
        [IsAccepted] bit NOT NULL,
        [RecordedOn] datetime2 NULL,
        [ExpiresOnUTC] datetime2 NULL,
        CONSTRAINT [PK_UserConsents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserConsents_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserConsents_UserAgreements_UserAgreementID] FOREIGN KEY ([UserAgreementID]) REFERENCES [UserAgreements] ([Id]) ON DELETE CASCADE
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE TABLE [OrganizationUnitMembers] (
        [Id] uniqueidentifier NOT NULL DEFAULT (newid()),
        [IsDeleted] bit NULL DEFAULT CAST(0 AS bit),
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL DEFAULT (getutcdate()),
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [OrganizationUnitId] uniqueidentifier NOT NULL,
        [PersonId] uniqueidentifier NOT NULL,
        [IsAdministrator] bit NULL,
        CONSTRAINT [PK_OrganizationUnitMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganizationUnitMembers_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrganizationUnitMembers_OrganizationUnits_OrganizationUnitId] FOREIGN KEY ([OrganizationUnitId]) REFERENCES [OrganizationUnits] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrganizationUnitMembers_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE NO ACTION
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Name', N'Value') AND [object_id] = OBJECT_ID(N'[ConfigurationValues]'))
        SET IDENTITY_INSERT [ConfigurationValues] ON;
    INSERT INTO [ConfigurationValues] ([Name], [Value])
    VALUES (N'BinaryObjects:Adapter', N'FileSystemAdapter'),
    (N'BinaryObjects:Path', N'BinaryObjects'),
    (N'BinaryObjects:StorageProvider', N'FileSystem.Default'),
    (N'Queue.Global:DefaultMaxRetryCount', N'2'),
    (N'App:MaxExportRecords', N'100'),
    (N'App:MaxReturnRecords', N'100'),
    (N'App:EnableSwagger', N'true');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Name', N'Value') AND [object_id] = OBJECT_ID(N'[ConfigurationValues]'))
        SET IDENTITY_INSERT [ConfigurationValues] OFF;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_AccessRequests_OrganizationId] ON [AccessRequests] ([OrganizationId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_AccessRequests_PersonId] ON [AccessRequests] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_EmailVerifications_PersonId] ON [EmailVerifications] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationMembers_OrganizationId] ON [OrganizationMembers] ([OrganizationId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationMembers_PersonId] ON [OrganizationMembers] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE UNIQUE INDEX [IX_Organizations_Name] ON [Organizations] ([Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationSettings_OrganizationId] ON [OrganizationSettings] ([OrganizationId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationUnitMembers_OrganizationId] ON [OrganizationUnitMembers] ([OrganizationId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationUnitMembers_OrganizationUnitId] ON [OrganizationUnitMembers] ([OrganizationUnitId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_OrganizationUnitMembers_PersonId] ON [OrganizationUnitMembers] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE UNIQUE INDEX [IX_OrganizationUnits_OrganizationId_Name] ON [OrganizationUnits] ([OrganizationId], [Name]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_PersonCredentials_PersonId] ON [PersonCredentials] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_PersonEmails_PersonId] ON [PersonEmails] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_PersonPhones_PersonId] ON [PersonPhones] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_UserConsents_PersonId] ON [UserConsents] ([PersonId]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    CREATE INDEX [IX_UserConsents_UserAgreementID] ON [UserConsents] ([UserAgreementID]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201028010151_V1Intial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20201028010151_V1Intial', N'3.1.6');
END;

GO

