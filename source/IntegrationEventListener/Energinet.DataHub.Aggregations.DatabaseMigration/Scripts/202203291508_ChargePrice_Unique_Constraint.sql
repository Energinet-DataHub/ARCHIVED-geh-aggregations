ALTER TABLE dbo.ChargePrice
	ALTER COLUMN ChargeKey NVARCHAR(255)

ALTER TABLE dbo.ChargePrice
  ADD CONSTRAINT chprUnique UNIQUE (ChargeKey, [Time])