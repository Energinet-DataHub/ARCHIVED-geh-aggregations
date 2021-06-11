CREATE SCHEMA Coordinator;
GO

CREATE TABLE [Coordinator].[Jobs]
(
    Id uniqueidentifier PRIMARY KEY DEFAULT newid() NOT NULL,
    DatabricksJobId nvarchar(max) NOT NULL,
    State nvarchar(max) NOT NULL,
    Created DateTimeOffset NOT NULL,
    Owner nvarchar(max) DEFAULT 'Unknown' NOT NULL,
    SnapshotPath nvarchar(max),
    ProcessType nvarchar(max) NOT NULL
)
GO

CREATE TABLE [Coordinator].[Results]
(
    JobId uniqueidentifier FOREIGN KEY REFERENCES [Coordinator].[Jobs](Id) NOT NULL,
    Name nvarchar(max) NOT NULL,
    Path nvarchar(max) NOT NULL
)
GO