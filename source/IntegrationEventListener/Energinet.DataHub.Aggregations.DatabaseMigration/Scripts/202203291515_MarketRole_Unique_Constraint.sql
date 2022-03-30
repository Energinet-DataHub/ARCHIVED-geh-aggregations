ALTER TABLE dbo.MarketRole
  ADD CONSTRAINT mrUnique UNIQUE (EnergySupplierId, MeteringPointId, FromDate)