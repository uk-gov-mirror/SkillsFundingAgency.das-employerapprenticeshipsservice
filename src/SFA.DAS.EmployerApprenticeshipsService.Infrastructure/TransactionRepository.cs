﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data.Entities.Transaction;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.Levy;
using SFA.DAS.EAS.Domain.Models.Payments;
using SFA.DAS.EAS.Domain.Models.Transaction;

namespace SFA.DAS.EAS.Infrastructure
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly IMapper _mapper;

        public TransactionRepository(LevyDeclarationProviderConfiguration configuration, IMapper mapper)
            : base(configuration)
        {
            _mapper = mapper;
        }

        public async Task<List<TransactionLine>> GetTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@accountId", accountId, DbType.Int64);
                parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
                parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

                return await c.QueryAsync<TransactionEntity>(
                    sql: "[employer_financial].[GetTransactionLines_ByAccountId]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return MapTransactions(result);
        }

        public async Task<List<TransactionLine>> GetTransactionDetailsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@accountId", accountId, DbType.Int64);
                parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
                parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

                return await c.QueryAsync<TransactionEntity>(
                    sql: "[employer_financial].[GetTransactionDetail_ByDateRange]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return MapTransactions(result);
        }

        public Task<List<TransactionLine>> GetCourseTransactionByDateRange(string courseName, long accountId, DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetPreviousTransactionsCount(long accountId, DateTime fromDate)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@accountId", accountId, DbType.Int64);
                parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);

                return await c.ExecuteScalarAsync<int>(
                    sql: "[employer_financial].[GetPreviousTransactionsCount]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return result;
        }

        private List<TransactionLine> MapTransactions(IEnumerable<TransactionEntity> transactionEntities)
        {
            var transactions = new List<TransactionLine>();

            foreach (var entity in transactionEntities)
            {
                switch (entity.TransactionType)
                {
                    case TransactionItemType.Declaration:
                    case TransactionItemType.TopUp:
                        transactions.Add(_mapper.Map<LevyDeclarationTransactionLine>(entity));
                        break;

                    case TransactionItemType.Payment:
                    case TransactionItemType.SFACoInvestment:
                    case TransactionItemType.EmployerCoInvestment:
                        transactions.Add(_mapper.Map<PaymentTransactionLine>(entity));
                        break;

                    case TransactionItemType.Unknown:
                        transactions.Add(_mapper.Map<TransactionLine>(entity));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return transactions;
        }
    }
}
