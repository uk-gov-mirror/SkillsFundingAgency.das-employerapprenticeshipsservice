using SFA.DAS.EAS.Domain.Entities.Account;

namespace SFA.DAS.EAS.Application.Queries.GetUserAccounts
{
    public class GetUserAccountsQueryResponse
    {
        public Accounts<Account> Accounts { get; set; }
    }
}