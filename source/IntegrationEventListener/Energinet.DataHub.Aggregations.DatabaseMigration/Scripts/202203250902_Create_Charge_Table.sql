CREATE TABLE [dbo].[Charge]
(
	[RowId] [uniqueidentifier] NOT NULL,
<<<<<<< HEAD
	[Id] [nvarchar](255) NOT NULL,
	[SenderProvidedChargeId] [nvarchar](max) NOT NULL,
	[Type] [int] NOT NULL,
	[OwnerId] [nvarchar](255) NOT NULL,
=======
	[ChargeKey] [nvarchar](max) NOT NULL,
	[ChargeId] [nvarchar](255) NOT NULL,
	[ChargeType] [int] NOT NULL,
	[ChargeOwnerId] [nvarchar](255) NOT NULL,
>>>>>>> parent of 71eea6f3 (Update source/IntegrationEventListener/Energinet.DataHub.Aggregations.DatabaseMigration/Scripts/202203250902_Create_Charge_Table.sql)
	[Resolution] [int] NOT NULL,
	[TaxIndicator] [bit] NOT NULL,
	[Currency] [nvarchar](50) NOT NULL,
	[FromDate] [datetime2](7) NOT NULL,
	[ToDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Charge] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO