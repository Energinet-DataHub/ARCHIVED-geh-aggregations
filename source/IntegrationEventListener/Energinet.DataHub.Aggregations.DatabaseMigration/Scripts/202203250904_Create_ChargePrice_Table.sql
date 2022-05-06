CREATE TABLE [dbo].[ChargePrice]
(
	[RowId] [uniqueidentifier] NOT NULL,
	[ChargeKey] [nvarchar](max) NOT NULL,
	[ChargePrice] [decimal](18, 8) NOT NULL,
	[Time] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ChargePrice] PRIMARY KEY CLUSTERED 
(
	[RowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO