ALTER TABLE dbo.ChargeLink
	ALTER COLUMN ChargeKey NVARCHAR(255)

ALTER TABLE dbo.ChargeLink
  ADD CONSTRAINT chlkUnique UNIQUE (ChargeKey, MeteringPointId, FromDate)