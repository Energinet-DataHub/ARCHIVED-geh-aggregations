IF OBJECT_ID(N'dbo.Charge', N'U') IS NOT NULL  
   DROP TABLE [dbo].[Charge];  
GO

IF OBJECT_ID(N'dbo.ChargeLink', N'U') IS NOT NULL  
   DROP TABLE [dbo].[ChargeLink];  
GO

IF OBJECT_ID(N'dbo.ChargePrice', N'U') IS NOT NULL  
   DROP TABLE [dbo].[ChargePrice];  
GO

IF OBJECT_ID(N'dbo.EnergySupplier', N'U') IS NOT NULL  
   DROP TABLE [dbo].[EnergySupplier];  
GO

IF OBJECT_ID(N'dbo.EsBrpRelation', N'U') IS NOT NULL  
   DROP TABLE [dbo].[EsBrpRelation];  
GO

IF OBJECT_ID(N'dbo.GridLossSysCorr', N'U') IS NOT NULL  
   DROP TABLE [dbo].[GridLossSysCorr];  
GO

IF OBJECT_ID(N'dbo.MeteringPoint', N'U') IS NOT NULL  
   DROP TABLE [dbo].[MeteringPoint];  
GO