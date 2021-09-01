USE [Coordinator]
GO

/****** Object:  Table [dbo].[Results]    Script Date: 01-09-2021 10:25:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Results]') AND type in (N'U'))
DROP TABLE [dbo].[Results]
    GO

    USE [Coordinator]
    GO

/****** Object:  Table [dbo].[Jobs]    Script Date: 01-09-2021 10:25:30 ******/
    IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Jobs]') AND type in (N'U'))
DROP TABLE [dbo].[Jobs]
    GO

CREATE TABLE [dbo].[Snapshot] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [FromDate] DATETIME2(7) NOT NULL,
    [ToDate] DATETIME2(7) NOT NULL,
    [CreatedDate] DATETIME2(7) NOT NULL,
    [Path] NVARCHAR(MAX) NOT NULL,
    [GridAreas] NVARCHAR(MAX) NULL,
    )

CREATE TABLE [dbo].[Job]  (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [DatabricksJobId] INT NOT NULL,
    [SnapshotId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY references [dbo].[Snapshot](Id),
    [State] NVARCHAR(128) NOT NULL,
    [JobType] NVARCHAR(128) NOT NULL,
    [ProcessType] NVARCHAR(128) NOT NULL,
    [Owner] NVARCHAR(128) NOT NULL,
    [CreateDate] DATETIME2(7) NOT NULL,
    [EcecutionEndDate] DATETIME2(7) NULL,
    [ProcessVariant] NVARCHAR(128) NULL
    )

CREATE TABLE [dbo].[Result] (
    [JobId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [dbo].[Job](Id),
    [Name] NVARCHAR(128) NOT NULL,
    [Path] NVARCHAR(MAX) NOT NULL,
    [State] NVARCHAR(128) NOT NULL
    )
