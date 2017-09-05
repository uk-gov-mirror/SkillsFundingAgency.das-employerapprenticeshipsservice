﻿CREATE PROCEDURE [employer_financial].[GetTransactionLines_ByAccountId]
	@accountId BIGINT,
	@fromDate datetime,
	@toDate datetime
AS

select 
    main.*
    ,bal.balance AS Balance
from
(
	select Sum(amount) as balance,accountid 
	from employer_financial.TransactionLine 
	WHERE AccountId = @accountId and transactiontype in (1,2,3) group by accountid) as bal
left join
(
    SELECT 
       tl.[AccountId]
      ,tl.TransactionType
	  ,MAX(tl.TransactionDate) as TransactionDate
      ,Sum(tl.Amount) as Amount
      ,tl.UkPrn
      ,tl.DateCreated
	  ,tl.SfaCoInvestmentAmount
	  ,tl.EmployerCoInvestmentAmount
	  ,ld.PayrollYear
	  ,ld.PayrollMonth
  FROM [employer_financial].[TransactionLine] tl
  LEFT JOIN [employer_financial].LevyDeclaration ld on ld.submissionid = tl.submissionid
  WHERE tl.AccountId = @accountId AND tl.DateCreated >= @fromDate AND DateCreated <= @toDate
  GROUP BY tl.DateCreated, tl.AccountId, tl.UKPRN, tl.SfaCoInvestmentAmount, tl.EmployerCoInvestmentAmount, tl.TransactionType, ld.PayrollMonth, ld.PayrollYear
) as main on main.AccountId = bal.AccountId
order by DateCreated desc, TransactionType desc, ukprn desc