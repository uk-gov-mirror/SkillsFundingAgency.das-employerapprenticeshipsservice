using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Domain.Data.Entities.Account;
using SFA.DAS.EAS.Domain.Models.Account;

namespace SFA.DAS.EAS.Domain.Data.Repositories
{
    public interface IEmployerAccountRepository
    {
        Task<Entities.Account.Account> GetAccountById(long id);
        Task<Entities.Account.Account> GetAccountByHashedId(string hashedAccountId);
        Task<Accounts<Entities.Account.Account>> GetAccounts(string toDate, int pageNumber, int pageSize);
        Task<AccountDetail> GetAccountDetailByHashedId(string hashedAccountId);
        Task<List<Entities.Account.Account>> GetAllAccounts();
        Task<List<AccountHistoryEntry>> GetAccountHistory(long accountId);
        Task RenameAccount(long id, string name);
    }
}
