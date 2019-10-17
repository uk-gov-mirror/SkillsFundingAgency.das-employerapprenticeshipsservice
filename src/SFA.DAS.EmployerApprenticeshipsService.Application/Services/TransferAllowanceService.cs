﻿using System.Threading.Tasks;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Transfers;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.Infrastructure.Extensions;

namespace SFA.DAS.EAS.Application.Services
{
    public class TransferAllowanceService : ITransferAllowanceService
    {
        private readonly EmployerFinanceDbContext _db;
        private readonly LevyDeclarationProviderConfiguration _configuration;

        public TransferAllowanceService(EmployerFinanceDbContext db, LevyDeclarationProviderConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<TransferAllowance> GetTransferAllowance(long accountId)
        {
            return _db.GetTransferAllowance(accountId, _configuration.TransferAllowancePercentage);
        }
    }
}