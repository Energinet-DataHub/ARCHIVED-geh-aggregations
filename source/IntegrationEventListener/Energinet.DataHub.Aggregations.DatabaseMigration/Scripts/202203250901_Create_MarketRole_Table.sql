CREATE TABLE [dbo].[MarketRole]
(
	[RowId] [uniqueidentifier] NOT NULL,
	[EnergySupplierId] [nvarchar](50) NOT NULL,
	[MeteringPointId] [nvarchar](50) NOT NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MarketRole] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO