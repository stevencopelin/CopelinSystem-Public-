CREATE TABLE [SubmissionNotificationDismissals] (
    [Id] int NOT NULL IDENTITY,
    [TokenId] uniqueidentifier NOT NULL,
    [UserId] int NOT NULL,
    [DismissedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_SubmissionNotificationDismissals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubmissionNotificationDismissals_SubmissionTokens_TokenId] FOREIGN KEY ([TokenId]) REFERENCES [SubmissionTokens] ([Token]) ON DELETE CASCADE,
    CONSTRAINT [FK_SubmissionNotificationDismissals_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE INDEX [IX_SubmissionNotificationDismissals_UserId] ON [SubmissionNotificationDismissals] ([UserId]);
CREATE UNIQUE INDEX [IX_SubmissionNotificationDismissals_UserId_TokenId] ON [SubmissionNotificationDismissals] ([UserId], [TokenId]);
