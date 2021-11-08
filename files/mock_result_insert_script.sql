INSERT INTO [dbo].[Snapshot] (Id, CreatedDate, FromDate, ToDate)
	VALUES ('e4383f79-f339-425a-a9da-eccecb564c1d', GETDATE(), '2021-10-01T22:00:00.0000000', '2021-10-02T22:00:00.0000000')

GO

INSERT INTO [dbo].[Job] (Id, SnapshotId, CreatedDate, Owner, Type, ProcessType, IsSimulation, DatabricksJobId, Resolution, StartedDate, CompletedDate, State)
	VALUES ('e7dbdf03-905a-437f-b609-67f45e99ad2b', 'e4383f79-f339-425a-a9da-eccecb564c1d', GETDATE(), 'System', 1, 1, 0, 0, 1, '2021-11-01T00:00:00.0000000', '2021-11-01T00:00:10.0000000', 2)

GO

INSERT INTO [dbo].[JobResult] (JobId, ResultId, Path, State)
SELECT
	'e7dbdf03-905a-437f-b609-67f45e99ad2b',
	r.Id,
	CONCAT('path/',r.Name),
	1
FROM
	[dbo].[Result] r
WHERE
	r.Type = 1

GO