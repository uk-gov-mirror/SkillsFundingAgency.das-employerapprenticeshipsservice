﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EntityFramework;

namespace SFA.DAS.EmployerAccounts.Queries.GetStatistics
{
    public class GetStatisticsQueryHandler : IAsyncRequestHandler<GetStatisticsQuery, GetStatisticsResponse>
    {
        private readonly Lazy<EmployerAccountsDbContext> _accountDb;

        public GetStatisticsQueryHandler(Lazy<EmployerAccountsDbContext> accountDb)
        {
            _accountDb = accountDb;
        }

        public async Task<GetStatisticsResponse> Handle(GetStatisticsQuery message)
        {
            //todo: make note in pr: adds reference to infrastructure
            var accountsQuery = _accountDb.Value.Accounts.FutureCount();
            var legalEntitiesQuery = _accountDb.Value.LegalEntities.FutureCount();
            var payeSchemesQuery = _accountDb.Value.Payees.FutureCount();
            var agreementsQuery = _accountDb.Value.Agreements.Where(a => a.StatusId == EmployerAgreementStatus.Signed).FutureCount();
            //var paymentsQuery = _financeDb.Payments.FutureCount();

            var statistics = new Statistics
            {
                TotalAccounts = await accountsQuery.ValueAsync(),
                TotalLegalEntities = await legalEntitiesQuery.ValueAsync(),
                TotalPayeSchemes = await payeSchemesQuery.ValueAsync(),
                TotalAgreements = await agreementsQuery.ValueAsync()
                //TotalPayments = await paymentsQuery.ValueAsync()
            };

            return new GetStatisticsResponse
            {
                Statistics = statistics
            };
        }
    }
}