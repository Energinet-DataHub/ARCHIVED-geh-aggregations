﻿DROP TABLE [dbo].[Result]
GO

DROP TABLE [dbo].[Job]
GO

CREATE TABLE [dbo].[Job](
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SnapshotId UNIQUEIDENTIFIER FOREIGN KEY (SnapshotId) REFERENCES [dbo].[Snapshot](Id),
    DatabricksJobId BIGINT NULL,
    [State] INT NOT NULL,
    [Type] INT NOT NULL,
    ProcessType INT NULL,
    ProcessVariant INT NULL,
    Resolution INT NULL,
    [Owner] NVARCHAR(128) NOT NULL,
    IsSimulation BIT NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL,
    StartedDate DATETIME2(7) NULL,
    CompletedDate DATETIME2(7) NULL,
    DeletedDate DATETIME2(7) NULL
)
GO

CREATE TABLE [dbo].[Result](
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    [Name] NVARCHAR(128) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    [Order] INT NOT NULL,
    [Type] INT NOT NULL,
    ConvertToXml BIT NOT NULL,
    [Grouping] INT NOT NULL,
    CreatedDate DATETIME2(7) NOT NULL,
    DeletedDate DATETIME2(7) NULL
)
GO

CREATE TABLE [dbo].[JobResult](
    JobId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY (JobId) REFERENCES [dbo].[Job](Id),
    ResultId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY (ResultId) REFERENCES [dbo].[Result](Id),
    [Path] NVARCHAR(MAX) NOT NULL,
    [State] INT NOT NULL,
    PRIMARY KEY(JobId,ResultId)
)
GO
