USE [CopelinSystem]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[external_region_emails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[external_region_emails](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RegionId] [int] NOT NULL,
        [Department] [nvarchar](50) NOT NULL,
        [EmailAddress] [nvarchar](255) NOT NULL,
        CONSTRAINT [PK_external_region_emails] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )
    )

    ALTER TABLE [dbo].[external_region_emails]  WITH CHECK ADD  CONSTRAINT [FK_external_region_emails_Regions_RegionId] FOREIGN KEY([RegionId])
    REFERENCES [dbo].[Regions] ([RegionId])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[external_region_emails] CHECK CONSTRAINT [FK_external_region_emails_Regions_RegionId]

    CREATE UNIQUE NONCLUSTERED INDEX [IX_external_region_emails_RegionId_Department] ON [dbo].[external_region_emails]
    (
        [RegionId] ASC,
        [Department] ASC
    )
END
GO
