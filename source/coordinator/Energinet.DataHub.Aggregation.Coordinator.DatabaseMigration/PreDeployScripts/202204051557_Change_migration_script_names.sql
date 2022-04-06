DELETE FROM dbo.SchemaVersions
WHERE ScriptName LIKE 'Energinet.DataHub.Aggregations.%'

UPDATE [dbo].[SchemaVersions]
SET	ScriptName = SUBSTRING(ScriptName, 69, LEN(ScriptName))
WHERE ScriptName LIKE 'Energinet.DataHub.Aggregation%' OR ScriptName LIKE 'GreenEnergyHub.Aggregation%'