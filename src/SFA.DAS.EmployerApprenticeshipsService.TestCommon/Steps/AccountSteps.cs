using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BoDi;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.HashingService;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.Steps
{
    [Binding]
    public class AccountSteps
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;

        public AccountSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;
        }

        [Given(@"user ([^ ]*) created account ([^ ]*)")]
        public async Task<Account> GivenUserCreatedAccount(string username, string accountName)
        {
            var user = _objectContext.Users[username];
            var db = _objectContainer.Resolve<EmployerAccountDbContext>();
            var hashingService = _objectContainer.Resolve<IHashingService>();
            var publicHashingService = _objectContainer.Resolve<IPublicHashingService>();
            var agreementTemplate = await db.AgreementTemplates.OrderByDescending(t => t.VersionNumber).FirstAsync();

            var account = user.CreateAccount(
                accountName,
                legalEntityCode,
                legalEntityName,
                legalEntityDateOfIncorporation,
                legalEntityRegisteredAddress,
                legalEntityStatus,
                legalEntityAgreementTemplate,
                payeReference);

            await db.SaveChangesAsync();

            _objectContext.Accounts.Add(accountName, account);

            return account;
        }
    }
}