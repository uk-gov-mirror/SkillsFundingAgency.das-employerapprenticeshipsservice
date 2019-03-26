USE [SFA.DAS.EAS.Employer_Financial.Database]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [employer_financial].[GetInDateLevyMonths] --todo this is a sketch name, reconsider 
(
	@currentDate DATETIME = NULL,
	@expiryPeriod INT = NULL
)
RETURNS @t TABLE (PayrollYear nvarchar, PayrollMonth int)
AS
BEGIN
	--inputs (remove when function)
	--DECLARE @currentDate DATETIME = '2019-02-07'-- CURRENT_TIMESTAMP --to be passed in
	--DECLARE @expiryPeriod INT = 18

	--conversion of inputs
	DECLARE @currentPayrollStartYear int = DATEPART(year, @currentDate)
	DECLARE @currentPayrollMonth int = DATEPART(month, @currentDate)
	DECLARE @expiryPeriodYearsPortion int = FLOOR(@expiryPeriod / 12)
	DECLARE @expiryPeriodMonthsPortion int = @expiryPeriod % 12

	--calculate year (initial calculation which might be changed by months calculation if we drop back a year)
	IF(DATEPART(month, @currentDate) < 4) --if true we are in the second calendar year of the payroll year
		SET @currentPayrollStartYear = @currentPayrollStartYear - 1

	SET @currentPayrollStartYear = @currentPayrollStartYear - 2000

	DECLARE @expiryPayrollStartYear INT = @currentPayrollStartYear - @expiryPeriodYearsPortion

	--calculate month
	DECLARE @expiryPayrollMonth INT = @currentPayrollMonth - @expiryPeriodMonthsPortion

	--if it's last year adjust it and the year to represent the correct month accordingly
	if(@expiryPayrollMonth < -1)
	BEGIN
		SET @expiryPayrollMonth = @expiryPayrollMonth + 12
		SET @expiryPayrollStartYear = @expiryPayrollStartYear - 1
	END


	--final year formatting
	DECLARE @expiryPayrollYear varchar(5) = CAST(@expiryPayrollStartYear as varchar) + '-' + CAST(@expiryPayrollStartYear + 1 as varchar)

	--test select statement
	--SELECT	@currentPayrollStartYear AS '@currentPayrollStartYear',
	--		@expiryPayrollStartYear AS '@expiryPayrollStartYear',
	--		@expiryPayrollYear AS '@expiryPayrollYear',
	--		@expiryPayrollMonth AS '@expiryPayrollMonth'

	INSERT INTO @t VALUES (@expiryPayrollYear, @expiryPayrollMonth)

	RETURN
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