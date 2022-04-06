ALTER TABLE dbo.Results ADD
	State nchar(255) NOT NULL DEFAULT ''
GO

ALTER TABLE dbo.Jobs ADD
	GridArea nchar(255) NOT NULL DEFAULT '',
    ProcessPeriodStart DateTimeOffset NULL,
    ProcessPeriodEnd DateTimeOffset NULL,
	JobType nchar(255) NOT NULL DEFAULT '',
    ExecutionEnd DateTimeOffset NULL
GO