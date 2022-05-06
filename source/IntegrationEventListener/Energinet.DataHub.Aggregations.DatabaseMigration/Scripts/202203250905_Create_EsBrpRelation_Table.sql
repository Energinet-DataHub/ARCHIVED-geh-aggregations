CREATE TABLE [dbo].[EsBrpRelation]
(
	[RowId] [uniqueidentifier] NOT NULL,
	[EnergySupplierId] [nvarchar](50) NOT NULL,
	[BalanceResponsibleId] [nvarchar](50) NOT NULL,
	[GridArea] [nvarchar](50) NOT NULL,
	[MeteringPointType] [int] NOT NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_EsBrpRelation] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO