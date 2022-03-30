ALTER TABLE dbo.GridLossSysCorr
  ADD CONSTRAINT glscUnique UNIQUE (MeteringPointId, GridArea, EnergySupplierId, FromDate)