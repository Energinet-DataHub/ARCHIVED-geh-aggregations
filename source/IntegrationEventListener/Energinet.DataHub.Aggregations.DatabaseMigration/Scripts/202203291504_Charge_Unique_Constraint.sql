﻿ALTER TABLE dbo.Charge
	ALTER COLUMN ChargeKey NVARCHAR(255)

ALTER TABLE dbo.Charge
  ADD CONSTRAINT chUnique UNIQUE (ChargeKey, FromDate)