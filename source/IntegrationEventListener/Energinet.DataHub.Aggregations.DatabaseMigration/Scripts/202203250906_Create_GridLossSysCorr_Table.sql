CREATE TABLE [dbo].[GridLossSysCorr]
(
	[RowId] [uniqueidentifier] NOT NULL,
	[MeteringPointId] [nvarchar](50) NOT NULL,
	[GridArea] [nvarchar](50) NOT NULL,
	[EnergySupplierId] [nvarchar](50) NOT NULL,
	[IsGridLoss] [bit] NOT NULL,
	[IsSystemCorrection] [bit] NOT NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_GridLossSysCorr] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO