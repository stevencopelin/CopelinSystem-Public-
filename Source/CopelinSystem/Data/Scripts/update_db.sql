IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251125033823_InitialDatabaseBaseline'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251125033823_InitialDatabaseBaseline', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251125035608_AddConsultantsTable'
)
BEGIN
    CREATE TABLE [consultants] (
        [ConsultantId] int NOT NULL IDENTITY,
        [BusinessName] nvarchar(255) NOT NULL,
        [Contact] nvarchar(255) NULL,
        [Email] nvarchar(255) NULL,
        [Phone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [Services] nvarchar(500) NULL,
        [SupplierNumber] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DateCreated] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_consultants] PRIMARY KEY ([ConsultantId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251125035608_AddConsultantsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251125035608_AddConsultantsTable', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126022224_AddContractorsTable'
)
BEGIN
    CREATE TABLE [contractors] (
        [ContractorId] int NOT NULL IDENTITY,
        [BusinessName] nvarchar(255) NOT NULL,
        [Contact] nvarchar(255) NULL,
        [Email] nvarchar(255) NULL,
        [Phone] nvarchar(50) NULL,
        [Address] nvarchar(500) NULL,
        [Services] nvarchar(500) NULL,
        [SupplierNumber] nvarchar(100) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DateCreated] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_contractors] PRIMARY KEY ([ContractorId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126022224_AddContractorsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126022224_AddContractorsTable', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    CREATE TABLE [regions] (
        [RegionId] int NOT NULL IDENTITY,
        [RegionName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_regions] PRIMARY KEY ([RegionId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    CREATE TABLE [employees] (
        [EmployeeId] int NOT NULL IDENTITY,
        [FullName] nvarchar(255) NOT NULL,
        [RegionId] int NULL,
        [Active] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_employees] PRIMARY KEY ([EmployeeId]),
        CONSTRAINT [FK_employees_regions_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [regions] ([RegionId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    CREATE TABLE [employee_roles] (
        [EmployeeRoleId] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [RoleType] int NOT NULL,
        CONSTRAINT [PK_employee_roles] PRIMARY KEY ([EmployeeRoleId]),
        CONSTRAINT [FK_employee_roles_employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [employees] ([EmployeeId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_employee_roles_EmployeeId_RoleType] ON [employee_roles] ([EmployeeId], [RoleType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    CREATE INDEX [IX_employees_RegionId] ON [employees] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126025542_AddEmployeeManagementTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126025542_AddEmployeeManagementTables', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126030153_AddDateCreatedToEmployee'
)
BEGIN
    ALTER TABLE [employees] ADD [DateCreated] datetime NOT NULL DEFAULT (GETDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126030153_AddDateCreatedToEmployee'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126030153_AddDateCreatedToEmployee', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    ALTER TABLE [clients] ADD [RegionId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    CREATE TABLE [client_contacts] (
        [ClientContactId] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [ContactName] nvarchar(255) NOT NULL,
        [ContactEmail] nvarchar(255) NULL,
        [ContactPhone] nvarchar(50) NULL,
        [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [DateCreated] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_client_contacts] PRIMARY KEY ([ClientContactId]),
        CONSTRAINT [FK_client_contacts_clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [clients] ([ClientId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    CREATE INDEX [IX_clients_RegionId] ON [clients] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    CREATE INDEX [IX_client_contacts_ClientId] ON [client_contacts] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    ALTER TABLE [clients] ADD CONSTRAINT [FK_clients_regions_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [regions] ([RegionId]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126032428_AddClientRegionAndContacts'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126032428_AddClientRegionAndContacts', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126233739_AddNewEmployeeRoleTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126233739_AddNewEmployeeRoleTypes', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251126235740_AddAdministratorRoleType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251126235740_AddAdministratorRoleType', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    CREATE TABLE [permissions] (
        [PermissionId] int NOT NULL IDENTITY,
        [PermissionName] nvarchar(100) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_permissions] PRIMARY KEY ([PermissionId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    CREATE TABLE [role_permissions] (
        [RolePermissionId] int NOT NULL IDENTITY,
        [RoleId] tinyint NOT NULL,
        [PermissionId] int NOT NULL,
        [IsGranted] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_role_permissions] PRIMARY KEY ([RolePermissionId]),
        CONSTRAINT [FK_role_permissions_permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [permissions] ([PermissionId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_permissions_PermissionName] ON [permissions] ([PermissionName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    CREATE INDEX [IX_role_permissions_PermissionId] ON [role_permissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    CREATE UNIQUE INDEX [IX_role_permissions_RoleId_PermissionId] ON [role_permissions] ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionName', N'Category', N'DisplayName', N'Description') AND [object_id] = OBJECT_ID(N'[permissions]'))
        SET IDENTITY_INSERT [permissions] ON;
    EXEC(N'INSERT INTO [permissions] ([PermissionName], [Category], [DisplayName], [Description])
    VALUES (N''ViewProjects'', N''Projects'', N''View Projects'', N''Can view project list and details''),
    (N''EditProjects'', N''Projects'', N''Edit Projects'', N''Can edit project information''),
    (N''DeleteProjects'', N''Projects'', N''Delete Projects'', N''Can delete projects''),
    (N''AssignProjects'', N''Projects'', N''Assign Projects'', N''Can assign users to projects''),
    (N''ViewTasks'', N''Tasks'', N''View Tasks'', N''Can view tasks''),
    (N''AddTasks'', N''Tasks'', N''Add Tasks'', N''Can add new tasks''),
    (N''EditTasks'', N''Tasks'', N''Edit Tasks'', N''Can edit existing tasks''),
    (N''DeleteTasks'', N''Tasks'', N''Delete Tasks'', N''Can delete tasks''),
    (N''ViewProductivity'', N''Productivity'', N''View Productivity'', N''Can view productivity entries''),
    (N''AddProductivity'', N''Productivity'', N''Add Productivity'', N''Can add productivity entries''),
    (N''EditProductivity'', N''Productivity'', N''Edit Productivity'', N''Can edit productivity entries''),
    (N''DeleteProductivity'', N''Productivity'', N''Delete Productivity'', N''Can delete productivity entries''),
    (N''ManageClients'', N''Clients'', N''Manage Clients'', N''Can manage client information''),
    (N''ManageConsultants'', N''Consultants'', N''Manage Consultants'', N''Can manage consultant information''),
    (N''ManageContractors'', N''Contractors'', N''Manage Contractors'', N''Can manage contractor information''),
    (N''ManageEmployees'', N''Employees'', N''Manage Employees'', N''Can manage employee information''),
    (N''ManageUsers'', N''Users'', N''Manage Users'', N''Can manage all user accounts and permissions'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionName', N'Category', N'DisplayName', N'Description') AND [object_id] = OBJECT_ID(N'[permissions]'))
        SET IDENTITY_INSERT [permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN

                    -- ReadOnly (1): Can only view
                    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                    SELECT 1, PermissionId, 1 FROM permissions WHERE PermissionName IN ('ViewProjects', 'ViewTasks', 'ViewProductivity');

                    -- Estimator (2): Can view and edit projects, add tasks/productivity
                    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                    SELECT 2, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                    ('ViewProjects', 'EditProjects', 'ViewTasks', 'AddTasks', 'EditTasks', 'ViewProductivity', 'AddProductivity', 'EditProductivity');

                    -- Manager (3): Estimator + delete + manage clients/consultants/contractors
                    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                    SELECT 3, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                    ('ViewProjects', 'EditProjects', 'DeleteProjects', 'AssignProjects',
                     'ViewTasks', 'AddTasks', 'EditTasks', 'DeleteTasks',
                     'ViewProductivity', 'AddProductivity', 'EditProductivity', 'DeleteProductivity',
                     'ManageClients', 'ManageConsultants', 'ManageContractors');

                    -- PrincipalEstimator (4): Manager + manage employees
                    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                    SELECT 4, PermissionId, 1 FROM permissions WHERE PermissionName IN 
                    ('ViewProjects', 'EditProjects', 'DeleteProjects', 'AssignProjects',
                     'ViewTasks', 'AddTasks', 'EditTasks', 'DeleteTasks',
                     'ViewProductivity', 'AddProductivity', 'EditProductivity', 'DeleteProductivity',
                     'ManageClients', 'ManageConsultants', 'ManageContractors', 'ManageEmployees');

                    -- Admin (5): All permissions
                    INSERT INTO role_permissions (RoleId, PermissionId, IsGranted)
                    SELECT 5, PermissionId, 1 FROM permissions;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251130202323_AddPermissionSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251130202323_AddPermissionSystem', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectWr');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectWr] = N'''' WHERE [ProjectWr] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectWr] nvarchar(255) NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT N'' FOR [ProjectWr];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var1 + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectName] = N'''' WHERE [ProjectName] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectName] nvarchar(200) NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT N'' FOR [ProjectName];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectLocation');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var2 + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectLocation] = N'''' WHERE [ProjectLocation] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectLocation] nvarchar(255) NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT N'' FOR [ProjectLocation];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectDescription');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var3 + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectDescription] = N'''' WHERE [ProjectDescription] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectDescription] nvarchar(max) NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT N'' FOR [ProjectDescription];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectClientRequired');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var4 + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectClientRequired] = ''0001-01-01'' WHERE [ProjectClientRequired] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectClientRequired] date NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT '0001-01-01' FOR [ProjectClientRequired];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[project_list]') AND [c].[name] = N'ProjectClientCompletion');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [project_list] DROP CONSTRAINT ' + @var5 + ';');
    EXEC(N'UPDATE [project_list] SET [ProjectClientCompletion] = ''0001-01-01'' WHERE [ProjectClientCompletion] IS NULL');
    ALTER TABLE [project_list] ALTER COLUMN [ProjectClientCompletion] date NOT NULL;
    ALTER TABLE [project_list] ADD DEFAULT '0001-01-01' FOR [ProjectClientCompletion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    CREATE TABLE [task_configurations] (
        [Id] int NOT NULL IDENTITY,
        [RegionId] int NOT NULL,
        [TaskName] nvarchar(255) NOT NULL,
        [Duration] int NOT NULL,
        [IsValueBased] bit NOT NULL,
        [ValueThresholds] nvarchar(max) NULL,
        CONSTRAINT [PK_task_configurations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_task_configurations_regions_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [regions] ([RegionId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    CREATE INDEX [IX_task_configurations_RegionId] ON [task_configurations] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203234216_AddTaskConfiguration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251203234216_AddTaskConfiguration', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207025419_AddProjectEmails'
)
BEGIN
    CREATE TABLE [project_emails] (
        [EmailId] int NOT NULL IDENTITY,
        [ProjectId] int NULL,
        [ProjectWr] nvarchar(255) NULL,
        [EmailSubject] nvarchar(500) NOT NULL,
        [EmailFrom] nvarchar(255) NOT NULL,
        [EmailTo] nvarchar(255) NULL,
        [EmailBody] nvarchar(max) NULL,
        [EmailBodyHtml] nvarchar(max) NULL,
        [ReceivedDate] datetime NOT NULL,
        [ProcessedDate] datetime NULL,
        [IsMatched] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedDate] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_project_emails] PRIMARY KEY ([EmailId]),
        CONSTRAINT [FK_project_emails_project_list_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [project_list] ([ProjectId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207025419_AddProjectEmails'
)
BEGIN
    CREATE TABLE [project_email_attachments] (
        [AttachmentId] int NOT NULL IDENTITY,
        [EmailId] int NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ContentType] nvarchar(100) NULL,
        [CreatedDate] datetime NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT [PK_project_email_attachments] PRIMARY KEY ([AttachmentId]),
        CONSTRAINT [FK_project_email_attachments_project_emails_EmailId] FOREIGN KEY ([EmailId]) REFERENCES [project_emails] ([EmailId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207025419_AddProjectEmails'
)
BEGIN
    CREATE INDEX [IX_project_email_attachments_EmailId] ON [project_email_attachments] ([EmailId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207025419_AddProjectEmails'
)
BEGIN
    CREATE INDEX [IX_project_emails_ProjectId] ON [project_emails] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207025419_AddProjectEmails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251207025419_AddProjectEmails', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207235334_AddUserProjectPreference'
)
BEGIN
    CREATE TABLE [user_project_preference] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ProjectId] int NOT NULL,
        [SortOrder] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_user_project_preference] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_user_project_preference_project_list_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [project_list] ([ProjectId]) ON DELETE CASCADE,
        CONSTRAINT [FK_user_project_preference_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207235334_AddUserProjectPreference'
)
BEGIN
    CREATE INDEX [IX_user_project_preference_ProjectId] ON [user_project_preference] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207235334_AddUserProjectPreference'
)
BEGIN
    CREATE UNIQUE INDEX [IX_user_project_preference_UserId_ProjectId] ON [user_project_preference] ([UserId], [ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251207235334_AddUserProjectPreference'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251207235334_AddUserProjectPreference', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212010245_AddHelpSystem'
)
BEGIN
    CREATE TABLE [HelpSections] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Order] int NOT NULL,
        CONSTRAINT [PK_HelpSections] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212010245_AddHelpSystem'
)
BEGIN
    CREATE TABLE [HelpArticles] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [MediaType] nvarchar(50) NOT NULL,
        [MediaUrl] nvarchar(500) NULL,
        [HelpSectionId] int NOT NULL,
        [Order] int NOT NULL,
        CONSTRAINT [PK_HelpArticles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HelpArticles_HelpSections_HelpSectionId] FOREIGN KEY ([HelpSectionId]) REFERENCES [HelpSections] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212010245_AddHelpSystem'
)
BEGIN
    CREATE INDEX [IX_HelpArticles_HelpSectionId] ON [HelpArticles] ([HelpSectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212010245_AddHelpSystem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251212010245_AddHelpSystem', N'10.0.0');
END;

COMMIT;
GO

