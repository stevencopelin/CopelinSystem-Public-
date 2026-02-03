BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260111231813_AddChecklists')
BEGIN
    CREATE TABLE [ChecklistTemplates] (
        [TemplateId] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [Version] nvarchar(20) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ChecklistTemplates] PRIMARY KEY ([TemplateId])
    );

    CREATE TABLE [ChecklistSections] (
        [SectionId] int NOT NULL IDENTITY,
        [TemplateId] int NOT NULL,
        [SectionName] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_ChecklistSections] PRIMARY KEY ([SectionId]),
        CONSTRAINT [FK_ChecklistSections_ChecklistTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [ChecklistTemplates] ([TemplateId]) ON DELETE CASCADE
    );

    CREATE TABLE [ChecklistQuestions] (
        [QuestionId] int NOT NULL IDENTITY,
        [SectionId] int NOT NULL,
        [QuestionText] nvarchar(max) NOT NULL,
        [QuestionType] nvarchar(max) NOT NULL,
        [IsRequired] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [HelpText] nvarchar(max) NULL,
        CONSTRAINT [PK_ChecklistQuestions] PRIMARY KEY ([QuestionId]),
        CONSTRAINT [FK_ChecklistQuestions_ChecklistSections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [ChecklistSections] ([SectionId]) ON DELETE CASCADE
    );

    CREATE TABLE [ChecklistQuestionOptions] (
        [OptionId] int NOT NULL IDENTITY,
        [QuestionId] int NOT NULL,
        [OptionText] nvarchar(max) NOT NULL,
        [DisplayOrder] int NOT NULL,
        CONSTRAINT [PK_ChecklistQuestionOptions] PRIMARY KEY ([OptionId]),
        CONSTRAINT [FK_ChecklistQuestionOptions_ChecklistQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [ChecklistQuestions] ([QuestionId]) ON DELETE CASCADE
    );

    CREATE TABLE [ProjectChecklists] (
        [ChecklistInstanceId] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [TemplateId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CompletedBy] nvarchar(max) NULL,
        [CompletedDate] datetime2 NULL,
        CONSTRAINT [PK_ProjectChecklists] PRIMARY KEY ([ChecklistInstanceId]),
        -- Changed to NO ACTION to prevent deletion of templates that are in use
        CONSTRAINT [FK_ProjectChecklists_ChecklistTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [ChecklistTemplates] ([TemplateId]) ON DELETE NO ACTION
    );

    CREATE TABLE [ChecklistResponses] (
        [ResponseId] int NOT NULL IDENTITY,
        [ChecklistInstanceId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [ResponseValue] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [ResponseDate] datetime2 NOT NULL,
        [RespondedBy] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ChecklistResponses] PRIMARY KEY ([ResponseId]),
        -- Changed to NO ACTION to avoid multiple cascade paths (Cycle)
        CONSTRAINT [FK_ChecklistResponses_ChecklistQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [ChecklistQuestions] ([QuestionId]) ON DELETE NO ACTION,
        -- Keep CASCADE here so deleting an instance deletes its responses
        CONSTRAINT [FK_ChecklistResponses_ProjectChecklists_ChecklistInstanceId] FOREIGN KEY ([ChecklistInstanceId]) REFERENCES [ProjectChecklists] ([ChecklistInstanceId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ChecklistQuestionOptions_QuestionId] ON [ChecklistQuestionOptions] ([QuestionId]);
    CREATE INDEX [IX_ChecklistQuestions_SectionId] ON [ChecklistQuestions] ([SectionId]);
    CREATE INDEX [IX_ChecklistResponses_ChecklistInstanceId] ON [ChecklistResponses] ([ChecklistInstanceId]);
    CREATE INDEX [IX_ChecklistResponses_QuestionId] ON [ChecklistResponses] ([QuestionId]);
    CREATE INDEX [IX_ChecklistSections_TemplateId] ON [ChecklistSections] ([TemplateId]);
    CREATE INDEX [IX_ProjectChecklists_TemplateId] ON [ProjectChecklists] ([TemplateId]);

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260111231813_AddChecklists', N'8.0.0');
END;

GO
COMMIT;
