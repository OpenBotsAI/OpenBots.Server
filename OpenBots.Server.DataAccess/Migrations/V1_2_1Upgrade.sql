IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DELETE FROM [ConfigurationValues]
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DROP TABLE [EmailLogs];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DROP TABLE [PersonPhones];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DROP TABLE [Processes];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DROP TABLE [ProcessExecutionLogs];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DROP TABLE [ProcessLogs];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] DROP CONSTRAINT [PK_ConfigurationValues];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''App:EnableSwagger'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''App:MaxExportRecords'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''App:MaxReturnRecords'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''BinaryObjects:Adapter'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''BinaryObjects:Path'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''BinaryObjects:StorageProvider'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC(N'DELETE FROM [ConfigurationValues]
    WHERE [Name] = N''Queue.Global:DefaultMaxRetryCount'';
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'IsHealthy');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Agents] DROP COLUMN [IsHealthy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'LastReportedMessage');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Agents] DROP COLUMN [LastReportedMessage];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'LastReportedOn');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Agents] DROP COLUMN [LastReportedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'LastReportedStatus');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Agents] DROP COLUMN [LastReportedStatus];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'LastReportedWork');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Agents] DROP COLUMN [LastReportedWork];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC sp_rename N'[Schedules].[ProcessId]', N'QueueId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    EXEC sp_rename N'[Jobs].[ProcessId]', N'AutomationVersionId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [Schedules] ADD [AutomationId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [QueueItems] ADD [ResultJSON] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [OrganizationSettings] ADD [IPFencingMode] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [Jobs] ADD [AutomationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [Jobs] ADD [AutomationVersion] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConfigurationValues]') AND [c].[name] = N'Name');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ConfigurationValues] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [ConfigurationValues] ALTER COLUMN [Name] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [CreatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [CreatedOn] datetime2 NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [DeleteOn] datetime2 NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [DeletedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [IsDeleted] bit NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [Timestamp] rowversion NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [UIHint] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [UpdatedBy] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [UpdatedOn] datetime2 NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD [ValidationRegex] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BinaryObjects]') AND [c].[name] = N'Name');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [BinaryObjects] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [BinaryObjects] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    ALTER TABLE [ConfigurationValues] ADD CONSTRAINT [PK_ConfigurationValues] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [AgentHeartbeats] (
        [Id] uniqueidentifier NOT NULL,
        [AgentId] uniqueidentifier NOT NULL,
        [LastReportedOn] datetime2 NULL,
        [LastReportedStatus] nvarchar(max) NULL,
        [LastReportedWork] nvarchar(max) NULL,
        [LastReportedMessage] nvarchar(max) NULL,
        [IsHealthy] bit NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_AgentHeartbeats] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [AutomationExecutionLogs] (
        [Id] uniqueidentifier NOT NULL,
        [JobID] uniqueidentifier NOT NULL,
        [AutomationID] uniqueidentifier NOT NULL,
        [AgentID] uniqueidentifier NOT NULL,
        [StartedOn] datetime2 NOT NULL,
        [CompletedOn] datetime2 NOT NULL,
        [Trigger] nvarchar(max) NULL,
        [TriggerDetails] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [HasErrors] bit NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [ErrorDetails] nvarchar(max) NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_AutomationExecutionLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [AutomationLogs] (
        [Id] uniqueidentifier NOT NULL,
        [Message] nvarchar(max) NULL,
        [MessageTemplate] nvarchar(max) NULL,
        [Level] nvarchar(max) NULL,
        [AutomationLogTimeStamp] datetime2 NULL,
        [Exception] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [JobId] uniqueidentifier NULL,
        [AutomationId] uniqueidentifier NULL,
        [AgentId] uniqueidentifier NULL,
        [MachineName] nvarchar(max) NULL,
        [AgentName] nvarchar(max) NULL,
        [AutomationName] nvarchar(max) NULL,
        [Logger] nvarchar(max) NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_AutomationLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [Automations] (
        [Id] uniqueidentifier NOT NULL,
        [BinaryObjectId] uniqueidentifier NOT NULL,
        [OriginalPackageName] nvarchar(max) NULL,
        [AutomationEngine] nvarchar(max) NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Automations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [AutomationVersions] (
        [Id] uniqueidentifier NOT NULL,
        [AutomationId] uniqueidentifier NOT NULL,
        [VersionNumber] int NOT NULL,
        [PublishedBy] nvarchar(max) NULL,
        [PublishedOnUTC] datetime2 NULL,
        [Status] nvarchar(max) NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_AutomationVersions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [EmailAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [ContentType] nvarchar(max) NULL,
        [SizeInBytes] bigint NULL,
        [ContentStorageAddress] nvarchar(max) NULL,
        [BinaryObjectId] uniqueidentifier NULL,
        [EmailId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_EmailAttachments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [Emails] (
        [Id] uniqueidentifier NOT NULL,
        [EmailAccountId] uniqueidentifier NULL,
        [SentOnUTC] datetime2 NULL,
        [EmailObjectJson] nvarchar(max) NULL,
        [SenderName] nvarchar(max) NULL,
        [SenderAddress] nvarchar(max) NULL,
        [SenderUserId] uniqueidentifier NULL,
        [Status] nvarchar(max) NULL,
        [Reason] nvarchar(max) NULL,
        [Direction] nvarchar(max) NULL,
        [ConversationId] uniqueidentifier NULL,
        [ReplyToEmailId] uniqueidentifier NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_Emails] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [IntegrationEventLogs] (
        [Id] uniqueidentifier NOT NULL,
        [IntegrationEventName] nvarchar(max) NOT NULL,
        [OccuredOnUTC] datetime2 NOT NULL,
        [EntityType] nvarchar(max) NULL,
        [EntityID] uniqueidentifier NULL,
        [PayloadJSON] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [SHA256Hash] nvarchar(max) NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_IntegrationEventLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [IntegrationEvents] (
        [Id] uniqueidentifier NOT NULL,
        [Description] nvarchar(2048) NULL,
        [EntityType] nvarchar(256) NULL,
        [PayloadSchema] nvarchar(max) NULL,
        [IsSystem] bit NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_IntegrationEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [IntegrationEventSubscriptionAttempts] (
        [Id] uniqueidentifier NOT NULL,
        [EventLogID] uniqueidentifier NOT NULL,
        [IntegrationEventSubscriptionID] uniqueidentifier NOT NULL,
        [IntegrationEventName] nvarchar(max) NULL,
        [Status] nvarchar(max) NULL,
        [AttemptCounter] int NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_IntegrationEventSubscriptionAttempts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [IntegrationEventSubscriptions] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(max) NULL,
        [IntegrationEventName] nvarchar(max) NULL,
        [EntityID] uniqueidentifier NULL,
        [EntityName] nvarchar(max) NULL,
        [TransportType] int NOT NULL,
        [HTTP_URL] nvarchar(max) NULL,
        [HTTP_AddHeader_Key] nvarchar(max) NULL,
        [HTTP_AddHeader_Value] nvarchar(max) NULL,
        [HTTP_Max_RetryCount] int NULL,
        [QUEUE_QueueID] uniqueidentifier NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_IntegrationEventSubscriptions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [IPFencings] (
        [Id] uniqueidentifier NOT NULL,
        [Usage] int NOT NULL,
        [Rule] int NOT NULL,
        [IPAddress] nvarchar(max) NULL,
        [IPRange] nvarchar(max) NULL,
        [HeaderName] nvarchar(max) NULL,
        [HeaderValue] nvarchar(max) NULL,
        [OrganizationId] uniqueidentifier NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_IPFencings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [JobCheckpoints] (
        [Id] uniqueidentifier NOT NULL,
        [Message] nvarchar(max) NULL,
        [Iterator] nvarchar(max) NULL,
        [IteratorValue] nvarchar(max) NULL,
        [IteratorPosition] int NULL,
        [IteratorCount] int NULL,
        [JobId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_JobCheckpoints] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [JobParameters] (
        [Id] uniqueidentifier NOT NULL,
        [DataType] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [JobId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_JobParameters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    CREATE TABLE [QueueItemAttachments] (
        [Id] uniqueidentifier NOT NULL,
        [QueueItemId] uniqueidentifier NOT NULL,
        [BinaryObjectId] uniqueidentifier NOT NULL,
        [IsDeleted] bit NULL,
        [CreatedBy] nvarchar(100) NULL,
        [CreatedOn] datetime2 NULL,
        [DeletedBy] nvarchar(100) NULL,
        [DeleteOn] datetime2 NULL,
        [Timestamp] rowversion NOT NULL,
        [UpdatedOn] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_QueueItemAttachments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedOn', N'DeleteOn', N'DeletedBy', N'Description', N'EntityType', N'IsDeleted', N'IsSystem', N'Name', N'PayloadSchema', N'UpdatedBy', N'UpdatedOn') AND [object_id] = OBJECT_ID(N'[IntegrationEvents]'))
        SET IDENTITY_INSERT [IntegrationEvents] ON;
    EXEC(N'INSERT INTO [IntegrationEvents] ([Id], [CreatedBy], [CreatedOn], [DeleteOn], [DeletedBy], [Description], [EntityType], [IsDeleted], [IsSystem], [Name], [PayloadSchema], [UpdatedBy], [UpdatedOn])
    VALUES (''744ba6f9-161f-41dc-b76e-c1602fc65d1b'', N'''', NULL, NULL, N'''', N''A Queue has been updated'', N''Queue'', CAST(0 AS bit), CAST(1 AS bit), N''Queues.QueueUpdated'', NULL, NULL, NULL),
    (''6ce8b3da-0373-4da2-bc77-ea845212855d'', N'''', NULL, NULL, N'''', N''A new agent has been created'', N''Agent'', CAST(0 AS bit), CAST(1 AS bit), N''Agents.NewAgentCreated'', NULL, NULL, NULL),
    (''35fd2aa3-6c77-4995-9ed8-9b262e5afdfc'', N'''', NULL, NULL, N'''', N''An Agent has reported an unhealthy status'', N''Agent'', CAST(0 AS bit), CAST(1 AS bit), N''Agents.UnhealthyReported'', NULL, NULL, NULL),
    (''6e0c741c-34b0-471e-a491-c7ec61782e94'', N'''', NULL, NULL, N'''', N''An Asset has been deleted'', N''Asset'', CAST(0 AS bit), CAST(1 AS bit), N''Assets.AssetDeleted'', NULL, NULL, NULL),
    (''4ce67735-2edc-4b7f-849a-5575740a496f'', N'''', NULL, NULL, N'''', N''An Asset has been updated'', N''Asset'', CAST(0 AS bit), CAST(1 AS bit), N''Assets.AssetUpdated'', NULL, NULL, NULL),
    (''f1b111cc-1f26-404d-827c-e30305c2ecc4'', N'''', NULL, NULL, N'''', N''A new Asset has been created'', N''Asset'', CAST(0 AS bit), CAST(1 AS bit), N''Assets.NewAssetCreated'', NULL, NULL, NULL),
    (''90f9f691-90e5-41d0-9b2c-1e8437bc85d3'', N'''', NULL, NULL, N'''', N''A Process has been deleted'', N''Automation'', CAST(0 AS bit), CAST(1 AS bit), N''Automations.AutomationDeleted'', NULL, NULL, NULL),
    (''8437fa1f-777a-4905-a169-feb32214c0c8'', N'''', NULL, NULL, N'''', N''A Process has been updated'', N''Automation'', CAST(0 AS bit), CAST(1 AS bit), N''Automations.AutomationUpdated'', NULL, NULL, NULL),
    (''93416738-3284-4bb0-869e-e2f191446b44'', N'''', NULL, NULL, N'''', N''A new Process has been created'', N''Automation'', CAST(0 AS bit), CAST(1 AS bit), N''Automations.NewAutomationCreated'', NULL, NULL, NULL),
    (''ecced501-9c35-4b37-a7b2-b6b901f91234'', N'''', NULL, NULL, N'''', N''A Credential has been deleted'', N''Credential'', CAST(0 AS bit), CAST(1 AS bit), N''Credentials.CredentialDeleted'', NULL, NULL, NULL),
    (''efd1d688-1881-4d5e-aed7-81528d54d7ef'', N'''', NULL, NULL, N'''', N''A Credential has been updated'', N''Credential'', CAST(0 AS bit), CAST(1 AS bit), N''Credentials.CredentialUpdated'', NULL, NULL, NULL),
    (''2b4bd195-62ac-4111-97ca-d6df6dd3f0fb'', N'''', NULL, NULL, N'''', N''An Agent has been updated'', N''Agent'', CAST(0 AS bit), CAST(1 AS bit), N''Agents.AgentUpdated'', NULL, NULL, NULL),
    (''76f6ab13-c430-46ad-b859-3d2dfd802e84'', N'''', NULL, NULL, N'''', N''A new Credential has been created'', N''Credential'', CAST(0 AS bit), CAST(1 AS bit), N''Credentials.NewCredentialCreated'', NULL, NULL, NULL),
    (''3ff9b456-7832-4499-b263-692c021e7d80'', N'''', NULL, NULL, N'''', N''A File has been updated'', N''File'', CAST(0 AS bit), CAST(1 AS bit), N''Files.FileUpdated'', NULL, NULL, NULL),
    (''04cf6a7a-ca72-48bc-887f-666ef580d198'', N'''', NULL, NULL, N'''', N''A new File has been created'', N''File'', CAST(0 AS bit), CAST(1 AS bit), N''Files.NewFileCreated'', NULL, NULL, NULL),
    (''82b8d08d-5ae2-4031-bdf8-5fba5597ac4b'', N'''', NULL, NULL, N'''', N''A Job has been deleted'', N''Job'', CAST(0 AS bit), CAST(1 AS bit), N''Jobs.JobsDeleted'', NULL, NULL, NULL),
    (''9d8e576a-a69d-43cf-bbc9-18103105d0a0'', N'''', NULL, NULL, N'''', N''A Job has been updated'', N''Job'', CAST(0 AS bit), CAST(1 AS bit), N''Jobs.JobUpdated'', NULL, NULL, NULL),
    (''06dd9940-a483-4a21-9551-cf2e32eeccae'', N'''', NULL, NULL, N'''', N''A new Job has been created'', N''Job'', CAST(0 AS bit), CAST(1 AS bit), N''Jobs.NewJobCreated'', NULL, NULL, NULL),
    (''30a8dcb9-10cf-43c6-a08f-b45fe2125dae'', N'''', NULL, NULL, N'''', N''A new QueueItem has been created'', N''QueueItem'', CAST(0 AS bit), CAST(1 AS bit), N''QueueItems.NewQueueItemCreated'', NULL, NULL, NULL),
    (''860689af-fd19-44ba-a5c7-53f6fed92065'', N'''', NULL, NULL, N'''', N''A QueueItem has been deleted'', N''QueueItem'', CAST(0 AS bit), CAST(1 AS bit), N''QueueItems.QueueItemDeleted'', NULL, NULL, NULL),
    (''0719a4c3-2143-4b9a-92ae-8b5a93075b98'', N'''', NULL, NULL, N'''', N''A QueueItem has been updated'', N''QueueItem'', CAST(0 AS bit), CAST(1 AS bit), N''QueueItems.QueueItemUpdated'', NULL, NULL, NULL),
    (''e9f64119-edbf-4779-a796-21ad59f76534'', N'''', NULL, NULL, N'''', N''A new Queue has been created'', N''Queue'', CAST(0 AS bit), CAST(1 AS bit), N''Queues.NewQueueCreated'', NULL, NULL, NULL),
    (''b00eeecd-5729-4f82-9cd2-dcfafd946965'', N'''', NULL, NULL, N'''', N''A Queue has been deleted'', N''Queue'', CAST(0 AS bit), CAST(1 AS bit), N''Queues.QueueDeleted'', NULL, NULL, NULL),
    (''32d63e9d-aa6e-481f-b928-541ddf979bdf'', N'''', NULL, NULL, N'''', N''A File has been deleted'', N''File'', CAST(0 AS bit), CAST(1 AS bit), N''Files.FileDeleted'', NULL, NULL, NULL),
    (''6ce0bb0e-cda1-49fa-a9e4-b67d904f826e'', N'''', NULL, NULL, N'''', N''An Agent has been deleted'', N''Agent'', CAST(0 AS bit), CAST(1 AS bit), N''Agents.AgentDeleted'', NULL, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedBy', N'CreatedOn', N'DeleteOn', N'DeletedBy', N'Description', N'EntityType', N'IsDeleted', N'IsSystem', N'Name', N'PayloadSchema', N'UpdatedBy', N'UpdatedOn') AND [object_id] = OBJECT_ID(N'[IntegrationEvents]'))
        SET IDENTITY_INSERT [IntegrationEvents] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201219125319_V1_2Upgrade')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20201219125319_V1_2Upgrade', N'5.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201221161155_seedForConfigurationValues')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Name', N'Value') AND [object_id] = OBJECT_ID(N'[ConfigurationValues]'))
        SET IDENTITY_INSERT [ConfigurationValues] ON;
    EXEC(N'INSERT INTO [ConfigurationValues] ([Name], [Value])
    VALUES (N''BinaryObjects:Adapter'', N''FileSystemAdapter''),
    (N''BinaryObjects:Path'', N''BinaryObjects''),
    (N''BinaryObjects:StorageProvider'', N''FileSystem.Default''),
    (N''Queue.Global:DefaultMaxRetryCount'', N''2''),
    (N''App:MaxExportRecords'', N''100''),
    (N''App:MaxReturnRecords'', N''100''),
    (N''App:EnableSwagger'', N''true'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Name', N'Value') AND [object_id] = OBJECT_ID(N'[ConfigurationValues]'))
        SET IDENTITY_INSERT [ConfigurationValues] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20201221161155_seedForConfigurationValues')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20201221161155_seedForConfigurationValues', N'5.0.0');
END;
GO

COMMIT;
GO

