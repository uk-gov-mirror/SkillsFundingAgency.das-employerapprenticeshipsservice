using MediatR;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetAccountBalances;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetAccountProviderPayments;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetAccountTransactions;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetEnglishFrationDetail;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetLastLevyDeclaration;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetPreviousTransactionsCount;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.Levy;
using SFA.DAS.EAS.Domain.Models.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Application.Services
{
    public class DasLevyService : IDasLevyService
    {
        private readonly IMediator _mediator;
        private readonly ITransactionRepository _transactionRepository;

        public DasLevyService(IMediator mediator, ITransactionRepository transactionRepository)
        {
            _mediator = mediator;
            _transactionRepository = transactionRepository;
        }

        public async Task<ICollection<TransactionLine>> GetAccountTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var result = await _mediator.SendAsync(new GetAccountTransactionsRequest
            {
                AccountId = accountId,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result.TransactionLines;
        }

        public async Task<ICollection<T>> GetAccountProviderPaymentsByDateRange<T>(
            long accountId, long ukprn, DateTime fromDate, DateTime toDate) where T : TransactionLine
        {
            var result = await _mediator.SendAsync(new GetAccountProviderPaymentsByDateRangeQuery
            {
                AccountId = accountId,
                UkPrn = ukprn,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result?.Transactions?.OfType<T>().ToList() ?? new List<T>();
        }

        public async Task<ICollection<AccountBalance>> GetAllAccountBalances()
        {
            var result = await _mediator.SendAsync(new GetAccountBalancesRequest());

            return result.Accounts;
        }

        public async Task<IEnumerable<DasEnglishFraction>> GetEnglishFractionHistory(long accountId, string empRef)
        {
            var result = await _mediator.SendAsync(new GetEnglishFractionDetailByEmpRefQuery
            {
                AccountId = accountId,
                EmpRef = empRef
            });

            return result.FractionDetail;
        }

        public async Task<int> GetPreviousAccountTransaction(long accountId, DateTime fromDate)
        {
            var result = await _mediator.SendAsync(new GetPreviousTransactionsCountRequest
            {
                AccountId = accountId,
                FromDate = fromDate
            });

            return result.Count;
        }

        public async Task<DasDeclaration> GetLastLevyDeclarationforEmpRef(string empRef)
        {
            var result = await _mediator.SendAsync(new GetLastLevyDeclarationQuery
            {
                EmpRef = empRef
            });

            return result.Transaction;
        }

        public Task<string> GetProviderName(int ukprn, long accountId, string periodEnd)
        {
            return _transactionRepository.GetProviderName(ukprn, accountId, periodEnd);
        }
    }
}