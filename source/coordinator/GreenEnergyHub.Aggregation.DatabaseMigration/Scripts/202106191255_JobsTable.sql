CREATE SCHEMA Coordinator;
GO

CREATE TABLE [Jobs]
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

CREATE TABLE [Results]
(
    JobId uniqueidentifier FOREIGN KEY REFERENCES [Jobs](Id) NOT NULL,
    Name nvarchar(max) NOT NULL,
    Path nvarchar(max) NOT NULL
)
GO
