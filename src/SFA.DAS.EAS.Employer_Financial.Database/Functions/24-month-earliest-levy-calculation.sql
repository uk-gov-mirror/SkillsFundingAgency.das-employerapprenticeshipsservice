USE [SFA.DAS.EAS.Employer_Financial.Database]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [employer_financial].[GetEarliestLevyDeclaration] --todo this is a sketch name, reconsider  - IF AMOUNT IS NEGATIVE AFTER COMPARING WITH PREVIOUS MONTH SET TO 0
(
	@currentDate DATETIME = NULL,
	@expiryPeriod INT = NULL,
	@payrollYear nvarchar(5) = NULL,
	@payrollMonth int = NULL
)
RETURNS @earliestLevyDeclaration TABLE 
	([Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[EmpRef] [nvarchar](50) NOT NULL,
	[LevyDueYTD] [decimal](18, 4) NULL,
	[LevyAllowanceForYear] [decimal](18, 4) NULL,
	[SubmissionDate] [datetime] NULL,
	[SubmissionId] [bigint] NOT NULL,
	[PayrollYear] [nvarchar](10) NULL,
	[PayrollMonth] [tinyint] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[EndOfYearAdjustment] [bit] NOT NULL,
	[EndOfYearAdjustmentAmount] [decimal](18, 4) NULL,
	[DateCeased] [datetime] NULL,
	[InactiveFrom] [datetime] NULL,
	[InactiveTo] [datetime] NULL,
	[HmrcSubmissionId] [bigint] NULL,
	[NoPaymentForPeriod] [bit] NULL)
AS
BEGIN
	--inputs (remove when function)
	--DECLARE @currentDate DATETIME = CURRENT_TIMESTAMP --to be passed in
	--DECLARE @expiryPeriod INT = 24
	--DECLARE @payrollYear = ''

	DECLARE @currentPayrollStartYear int = DATEPART(year, @currentDate)
	DECLARE @currentPayrollMonth int = DATEPART(month, @currentDate)

	DECLARE @monthAfterPayrollExpiry TABLE(monthPortion int, yearPotion int)

	INSERT INTO @monthAfterPayrollExpiry SELECT * FROM [employer_financial].[GetMonthAfterPayrollExpiry](@currentDate, @expiryPeriod)

	DECLARE @expiryPayrollMonth INT = (SELECT monthPortion FROM @monthAfterPayrollExpiry)
	DECLARE @expiryPayrollStartYear INT = (SELECT yearPotion FROM @monthAfterPayrollExpiry)

	INSERT INTO @earliestLevyDeclaration 
	SELECT 
		[AccountId]
		,[EmpRef]
		,ld.LevyDueYTD
		,[LevyAllowanceForYear]
		,[SubmissionDate]
		,[SubmissionId]
		,[PayrollYear]
		,[PayrollMonth]
		,[CreatedDate]
		,[EndOfYearAdjustment]
		,[EndOfYearAdjustmentAmount]
		,[DateCeased]
		,[InactiveFrom]
		,[InactiveTo]
		,[HmrcSubmissionId]
		,[NoPaymentForPeriod]
	FROM [SFA.DAS.EAS.Employer_Financial.Database].[employer_financial].[LevyDeclaration] ld
	WHERE 

	--DECLARE @adjustedExpiryPeriod int = @expiryPeriod -1

	----conversion of inputs
	--DECLARE @currentPayrollStartYear int = DATEPART(year, @currentDate)
	--DECLARE @currentPayrollMonth int = DATEPART(month, @currentDate)
	--DECLARE @expiryPeriodYearsPortion int = FLOOR(@adjustedExpiryPeriod / 12)
	--DECLARE @expiryPeriodMonthsPortion int = @adjustedExpiryPeriod % 12

	----calculate year (initial calculation which might be changed by months calculation if we drop back a year)
	--IF(DATEPART(month, @currentDate) < 4) --if true we are in the second calendar year of the payroll year
	--	SET @currentPayrollStartYear = @currentPayrollStartYear - 1

	--SET @currentPayrollStartYear = @currentPayrollStartYear - 2000

	--DECLARE @expiryPayrollStartYear INT = @currentPayrollStartYear - @expiryPeriodYearsPortion

	----calculate month
	--DECLARE @expiryPayrollMonth INT = @currentPayrollMonth - @expiryPeriodMonthsPortion

	----if it's last year adjust it and the year to represent the correct month accordingly
	--if(@expiryPayrollMonth < -1)
	--BEGIN
	--	SET @expiryPayrollMonth = @expiryPayrollMonth + 12
	--	SET @expiryPayrollStartYear = @expiryPayrollStartYear - 1
	--END

	----actual checks
	--DECLARE @payrollStartYear AS INT = CAST(LEFT(@payrollYear, 2) AS INT)

	----year is later, definitely in date
	--IF(@payrollStartYear > @expiryPayrollStartYear) RETURN 1

	----year is same, month is later, in date
	--IF(@payrollStartYear = @expiryPayrollStartYear AND @payrollMonth > @expiryPayrollMonth) RETURN 1

	----otherwise out of date
	--RETURN 0

	

	--final year formatting
	--DECLARE @expiryPayrollYear varchar(5) = CAST(@expiryPayrollStartYear as varchar) + '-' + CAST(@expiryPayrollStartYear + 1 as varchar)

	--test select statement
	--SELECT	@currentPayrollStartYear AS '@currentPayrollStartYear',
	--		@expiryPayrollStartYear AS '@expiryPayrollStartYear',
	--		@expiryPayrollYear AS '@expiryPayrollYear',
	--		@expiryPayrollMonth AS '@expiryPayrollMonth'

	--INSERT INTO @t VALUES (@expiryPayrollYear, @expiryPayrollMonth)
END
		
	--DECLARE @currentMonth Datetime = CURRENT_TIMESTAMP
	
	--SET @asOfDate = ISNULL(@asOfDate, GETDATE())

	--DECLARE @asOfYear INT = DATEPART(year, @asOfDate)
	--DECLARE @asOfMonth INT = DATEPART(month, @asOfDate)
	--DECLARE @asOfDay INT = DATEPART(day, @asOfDate)

--	IF(@asOfMonth = 4 AND @asOfDay >= 20 OR @asOfMonth > 4)
--		INSERT INTO @t VALUES (DATEFROMPARTS(@asOfYear - 1, 4, 20), DATEFROMPARTS(@asOfYear, 4, 20))
--	ELSE
--		INSERT INTO @t VALUES (DATEFROMPARTS(@asOfYear - 2, 4, 20), DATEFROMPARTS(@asOfYear - 1, 4, 20))
--	RETURN
--END

--SELECT * FROM [employer_financial].[GetInDateLevyMonths] @currentDate = CURRENT_TIMESTAMP, @expiryPeriod = 24