DELETE FROM dbo.SchemaVersions
WHERE ScriptName LIKE 'Energinet.DataHub.Aggregations.%'

UPDATE [dbo].[SchemaVersions]
SET	ScriptName = SUBSTRING(ScriptName, 54, LEN(ScriptName))
WHERE ScriptName LIKE 'GreenEnergyHub.Aggregation%'