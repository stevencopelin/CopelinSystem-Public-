BEGIN TRANSACTION;
CREATE TABLE [app_branding] (
    [id] int NOT NULL IDENTITY,
    [footer_html] nvarchar(max) NOT NULL,
    [is_locked] bit NOT NULL,
    CONSTRAINT [PK_app_branding] PRIMARY KEY ([id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'footer_html', N'is_locked') AND [object_id] = OBJECT_ID(N'[app_branding]'))
    SET IDENTITY_INSERT [app_branding] ON;
INSERT INTO [app_branding] ([id], [footer_html], [is_locked])
VALUES (1, CONCAT(CAST(N'<footer class="main-footer">' AS nvarchar(max)), nchar(10), N'    <strong> {{Year}} <a href="#">Estimating Module | Copelin System</a> - </strong>', nchar(10), N'    Qld Governement - QBuild.', nchar(10), N'    <div class="float-right d-none d-sm-inline-block">', nchar(10), N'        <b>Version</b> {{Version}}', nchar(10), N'    </div>', nchar(10), N'</footer>'), CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'footer_html', N'is_locked') AND [object_id] = OBJECT_ID(N'[app_branding]'))
    SET IDENTITY_INSERT [app_branding] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251216025831_AddAppBranding', N'10.0.0');

COMMIT;
GO

