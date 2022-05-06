ALTER TABLE dbo.MeteringPoint
  ADD CONSTRAINT mpUnique UNIQUE (MeteringPointId, FromDate)