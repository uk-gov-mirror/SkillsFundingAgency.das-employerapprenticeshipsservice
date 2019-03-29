CREATE TABLE [employer_financial].[UnfilteredTransactionLine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[SubmissionId] [bigint] NULL,
	[TransactionDate] [datetime] NOT NULL,
	[TransactionType] [tinyint] NOT NULL,
	[LevyDeclared] [decimal](18, 4) NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[EmpRef] [nvarchar](50) NULL,
	[PeriodEnd] [nvarchar](50) NULL,
	[Ukprn] [bigint] NULL,
	[SfaCoInvestmentAmount] [decimal](18, 4) NOT NULL,
	[EmployerCoInvestmentAmount] [decimal](18, 4) NOT NULL,
	[EnglishFraction] [decimal](18, 5) NULL,
	[TransferSenderAccountId] [bigint] NULL,
	[TransferSenderAccountName] [nvarchar](100) NULL,
	[TransferReceiverAccountId] [bigint] NULL,
	[TransferReceiverAccountName] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [employer_financial].[UnfilteredTransactionLine] ADD  DEFAULT ((0)) FOR [TransactionType]
GO

ALTER TABLE [employer_financial].[UnfilteredTransactionLine] ADD  DEFAULT ((0)) FOR [Amount]
GO

ALTER TABLE [employer_financial].[UnfilteredTransactionLine] ADD  DEFAULT ((0)) FOR [SfaCoInvestmentAmount]
GO

ALTER TABLE [employer_financial].[UnfilteredTransactionLine] ADD  DEFAULT ((0)) FOR [EmployerCoInvestmentAmount]
GO