﻿using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Caches;
using SFA.DAS.EmployerAccounts.Data;

namespace SFA.DAS.EmployerAccounts.Features
{
    public class AgreementService : IAgreementService
    {
        private const int NullCacheValue = -1;

        private readonly Lazy<EmployerAccountsDbContext> _db;
        private readonly IDistributedCache _cache;

        public AgreementService(Lazy<EmployerAccountsDbContext> db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<int?> GetAgreementVersionAsync(long accountId)
        {
            var version = await _cache.GetOrAddAsync(GetCacheKeyForAccount(accountId), k => GetMinAgreementVersionAsync(accountId)).ConfigureAwait(false);

            if (version == NullCacheValue)
            {
                return null;
            }

            return version;
        }

        private async Task<int> GetMinAgreementVersionAsync(long accountId)
        {
            return await _db.Value.AccountLegalEntities
                .WithSignedOrPendingAgreementsForAccount(accountId)
                .Select(ale => ale.SignedAgreementId == null ? 0 : ale.SignedAgreementVersion ?? 0)
                .DefaultIfEmpty(NullCacheValue)
                .MinAsync()
                .ConfigureAwait(false);
        }

        public Task RemoveFromCacheAsync(long accountId)
        {
            return _cache.RemoveFromCache(GetCacheKeyForAccount(accountId));
        }

        private string GetCacheKeyForAccount(long accountId)
        {
            return "AccountId:" + accountId.ToString(CultureInfo.InvariantCulture);
        }
    }
}