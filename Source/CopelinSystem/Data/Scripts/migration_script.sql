BEGIN TRANSACTION;
ALTER TABLE [SubmissionTokens] ADD [IsNotificationDismissed] bit NOT NULL DEFAULT CAST(0 AS bit);

CREATE TABLE [external_region_emails] (
    [Id] int NOT NULL IDENTITY,
    [RegionId] int NOT NULL,
    [Department] nvarchar(50) NOT NULL,
    [EmailAddress] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_external_region_emails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_external_region_emails_regions_RegionId] FOREIGN KEY ([RegionId]) REFERENCES [regions] ([RegionId]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_external_region_emails_RegionId_Department] ON [external_region_emails] ([RegionId], [Department]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230032507_DismissSubmissionNotification', N'10.0.0');

COMMIT;
GO

