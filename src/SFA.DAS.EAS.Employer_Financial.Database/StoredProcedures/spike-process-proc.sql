USE [SFA.DAS.EAS.Employer_Financial.Database]
GO
/****** Object:  StoredProcedure [employer_financial].[ProcessDeclarationsTransactions]    Script Date: 26/03/2019 14:23:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [employer_financial].[ProcessDeclarationsTransactions]
	@AccountId BIGINT,
	@EmpRef NVARCHAR(50),
	@currentDate DATETIME = NULL,
	@expiryPeriod INT = NULL
AS

-- Get Earliest Positive Levy Dec
DECLARE @EarliestPositiveLevyDecYear varchar(5)
DECLARE @EarliestPositiveLevyDecMonth int


SELECT TOP 1 @EarliestPositiveLevyDecYear = PayrollYear, @EarliestPositiveLevyDecMonth = PayrollMonth
FROM [employer_financial].[GetLevyDeclarationAndTopUp]
WHERE [employer_financial].[IsInDateLevy](@currentDate, @expiryPeriod, PayrollYear, PayrollMonth) = 1
AND LevyDeclaredInMonth > 0
ORDER BY PayrollYear, PayrollMonth
	

--Add the topup from the declaration
INSERT INTO [employer_financial].LevyDeclarationTopup
	select mainUpdate.* from
	(

	select 
		x.AccountId,
		DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
		x.SubmissionId,
		x.SubmissionDate,
		x.TopUp as Amount
	FROM 
		[employer_financial].[GetLevyDeclarationAndTopUp] x
	where
		x.LevyDueYTD is not null AND x.LastSubmission = 1 AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
	AND @EarliestPositiveLevyDecYear IS NOT NULL
	AND x.PayrollYear >= @EarliestPositiveLevyDecYear
	AND @EarliestPositiveLevyDecMonth IS NOT NULL
	AND x.PayrollMonth >= @EarliestPositiveLevyDecMonth
	union all
	select
		x.AccountId,
		DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
		x.SubmissionId,
		x.SubmissionDate,
		((x.EndOfYearAdjustmentAmount * isnull(x.EnglishFraction,0))* isnull(x.TopUpPercentage,0) * -1) as Amount
	FROM 
		[employer_financial].[GetLevyDeclarationAndTopUp] x
	where
		x.LevyDueYTD is not null and x.EndOfYearAdjustment = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
	AND @EarliestPositiveLevyDecYear IS NOT NULL
	AND x.PayrollYear >= @EarliestPositiveLevyDecYear
	AND @EarliestPositiveLevyDecMonth IS NOT NULL
	AND x.PayrollMonth >= @EarliestPositiveLevyDecMonth
	) mainUpdate
	inner join (
		select SubmissionId from [employer_financial].LevyDeclaration
	EXCEPT
		select SubmissionId from [employer_financial].LevyDeclarationTopup
	) dervx on dervx.SubmissionId = mainUpdate.SubmissionId


-- Create Declarations

INSERT INTO [employer_financial].TransactionLine
select mainUpdate.* from
	(
	select 
			x.AccountId,
			DATEFROMPARTS(DatePart(yyyy,x.CreatedDate),DatePart(MM,x.CreatedDate),DATEPART(dd,x.CreatedDate)) as DateCreated,
			x.SubmissionId as SubmissionId,
			x.SubmissionDate as TransactionDate,
			1 as TransactionType,
			x.LevyDeclaredInMonth as LevyDeclared,
			x.TotalAmount as Amount,		
			x.EmpRef as EmpRef,
			null as PeriodEnd,
			null as Ukprn,
			0 as SfaCoInvestmentAmount,
			0 as EmployerCoInvestmentAmount,
			x.EnglishFraction,
			null as TransferSenderAccountId,
			null as TransferSenderAccountName,
			null as TransferReceiverAccountId,
			null as TransferReceiverAccountName	
		FROM 
			[employer_financial].[GetLevyDeclarationAndTopUp] x
		where
			x.LevyDueYTD is not null AND x.LastSubmission = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
		AND @EarliestPositiveLevyDecYear IS NOT NULL
		AND x.PayrollYear >= @EarliestPositiveLevyDecYear
		AND @EarliestPositiveLevyDecMonth IS NOT NULL
		AND x.PayrollMonth >= @EarliestPositiveLevyDecMonth
	union all	
		select 
			x.AccountId,
			DATEFROMPARTS(DatePart(yyyy,x.CreatedDate),DatePart(MM,x.CreatedDate),DATEPART(dd,x.CreatedDate)) as DateCreated,
			x.SubmissionId as SubmissionId,
			x.SubmissionDate as TransactionDate,
			1 as TransactionType,
			x.EndOfYearAdjustmentAmount * -1 as LevyDeclared,
			((x.EndOfYearAdjustmentAmount * ISNULL(x.EnglishFraction,1)) - ldt.Amount) * -1 as Amount,
			x.EmpRef as EmpRef,
			null as PeriodEnd,
			null as Ukprn,
			0 as SfaCoInvestmentAmount,
			0 as EmployerCoInvestmentAmount,
			ISNULL(x.EnglishFraction,1) AS EnglishFraction,
			null as TransferSenderAccountId,
			null as TransferSenderAccountName,
			null as TransferReceiverAccountId,
			null as TransferReceiverAccountName			
		FROM 
			[employer_financial].[GetLevyDeclarationAndTopUp] x 
		inner join
			[employer_financial].[LevyDeclarationTopup] ldt on ldt.SubmissionId = x.SubmissionId
		where x.EndOfYearAdjustment = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
		AND @EarliestPositiveLevyDecYear IS NOT NULL
		AND x.PayrollYear >= @EarliestPositiveLevyDecYear
		AND @EarliestPositiveLevyDecMonth IS NOT NULL
		AND x.PayrollMonth >= @EarliestPositiveLevyDecMonth
	) mainUpdate
	inner join (
		select SubmissionId from [employer_financial].LevyDeclaration
	EXCEPT
		select SubmissionId from [employer_financial].TransactionLine where TransactionType = 1
	) dervx on dervx.SubmissionId = mainUpdate.SubmissionId

	--select * from [employer_financial].[GetLevyDeclarationAndTopUp] ldatu
	--WHERE [employer_financial].[IsInDateLevy](CURRENT_TIMESTAMP, 6, ldatu.PayrollYear, ldatu.PayrollMonth) = 1
	--ORDER BY PayrollYear, PayrollMonth

	----gets and compares payroll year of passed in expiry period
	----filters which levy decs are in (excluding last month)
	----constructs line for last month which can be unioned with the others

	--select * from [employer_financial].[GetLevyDeclarationAndTopUp]
	--WHERE AccountId = 4
	--ORDER BY PayrollYear, PayrollMonth