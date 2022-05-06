CREATE TABLE [dbo].[MeteringPoint]
(
	[RowId] [uniqueidentifier] NOT NULL,
	[MeteringPointId] [nvarchar](50) NOT NULL,
	[MeteringPointType] [int] NOT NULL,
	[SettlementMethod] [int] NULL,
	[GridArea] [nvarchar](50) NOT NULL,
	[ConnectionState] [int] NOT NULL,
	[Resolution] [int] NOT NULL,
	[InGridArea] [nvarchar](50) NULL,
	[OutGridArea] [nvarchar](50) NULL,
	[MeteringMethod] [int] NOT NULL,
	[ParentMeteringPointId] [nvarchar](50) NULL,
	[Unit] [int] NOT NULL,
	[Product] [nvarchar](50) NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MeteringPoint] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO