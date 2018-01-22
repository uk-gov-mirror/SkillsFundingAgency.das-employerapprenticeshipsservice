using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EAS.Account.Api.Client
{
    public class AccountApiClient2 : IAccountApiClient
    {
        private readonly HttpClient _client;

        public AccountApiClient2(HttpClient client)
        {
            _client = client;
        }

        public async Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            var url = $"/api/accounts/{hashedAccountId}";

            var json = await _client.GetStringAsync(url);

            return JsonConvert.DeserializeObject<AccountDetailViewModel>(json);
        }

        public async Task<AccountDetailViewModel> GetAccount(long accountId)
        {
            var url = $"/api/accounts/internal/{accountId}";

            var json = await _client.GetStringAsync(url);

            return JsonConvert.DeserializeObject<AccountDetailViewModel>(json);
        }

        public async Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> GetPageOfAccounts(int pageNumber = 1, int pageSize = 1000, DateTime? toDate = null)
        {
            var url = $"/api/accounts?pageNumber={pageNumber}&pageSize={pageSize}";
            if (toDate.HasValue)
            {
                var formattedToDate = toDate.Value.ToString("yyyyMMdd");
                url += $"&toDate={formattedToDate}";
            }

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<PagedApiResponseViewModel<AccountWithBalanceViewModel>>(json);
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            var url = $"/api/accounts/{accountId}/users";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }

        public async Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            var url = $"/api/accounts/internal/{accountId}/users";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ICollection<TeamMemberViewModel>>(json);
        }

        public async Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        {
            var url = $"/api/user/{userId}/accounts";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ICollection<AccountDetailViewModel>>(json);
        }

        public async Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id)
        {
            var url = $"/api/accounts/{accountId}/legalentities/{id}";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<LegalEntityViewModel>(json);
        }

        public async Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId)
        {
            var url = $"/api/accounts/{accountId}/legalentities";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<ResourceViewModel>>(json);
        }

        public async Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId)
        {
            var url = $"/api/accounts/{accountId}/payeschemes";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<ResourceViewModel>>(json);
        }

        public async Task<EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId)
        {
            var url = $"/api/accounts/{accountId}/legalEntities/{legalEntityId}/agreements/{agreementId}/agreement";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<EmployerAgreementView>(json);
        }


        public async Task<T> GetResource<T>(string uri) where T : IAccountResource
        {
            var json = await _client.GetStringAsync(uri);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<ICollection<LevyDeclarationViewModel>> GetLevyDeclarations(string accountId)
        {
            var url = $"/api/accounts/{accountId}/levy";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<List<LevyDeclarationViewModel>>(json);
        }

        public async Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month)
        {
            var url = $"/api/accounts/{accountId}/transactions/{year}/{month}";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<TransactionsViewModel>(json);
        }

        public async Task<TransactionsViewModel> GetTransactions(string accountId)
        {
            var url = $"/api/accounts/{accountId}/transactions";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<TransactionsViewModel>(json);
        }

        public async Task<ICollection<TransactionSummaryViewModel>> GetTransactionSummary(string accountId)
        {
            var url = $"/api/accounts/{accountId}/transactions";

            var json = await _client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<ICollection<TransactionSummaryViewModel>>(json);
        }
    }
}