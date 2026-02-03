USE [CopelinSystem]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FileSystemItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[ParentId] [int] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[IsFolder] [bit] NOT NULL,
	[PhysicalPath] [nvarchar](max) NULL,
	[ContentType] [nvarchar](max) NULL,
	[Size] [bigint] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_FileSystemItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FileSystemItems]  WITH CHECK ADD  CONSTRAINT [FK_FileSystemItems_ProjectLists_ProjectId] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[project_list] ([ProjectId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FileSystemItems] CHECK CONSTRAINT [FK_FileSystemItems_ProjectLists_ProjectId]
GO

ALTER TABLE [dbo].[FileSystemItems]  WITH CHECK ADD  CONSTRAINT [FK_FileSystemItems_FileSystemItems_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[FileSystemItems] ([Id])
GO

ALTER TABLE [dbo].[FileSystemItems] CHECK CONSTRAINT [FK_FileSystemItems_FileSystemItems_ParentId]
GO

CREATE NONCLUSTERED INDEX [IX_FileSystemItems_ProjectId] ON [dbo].[FileSystemItems]
(
	[ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_FileSystemItems_ParentId] ON [dbo].[FileSystemItems]
(
	[ParentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
