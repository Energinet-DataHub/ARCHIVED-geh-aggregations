ALTER TABLE dbo.EsBrpRelation
  ADD CONSTRAINT ebrUnique UNIQUE (EnergySupplierId, BalanceResponsibleId, GridArea, MeteringPointType, FromDate)