using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLevel;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountLevelTests
{
    [TestFixture]
    public class WhenIGetAccountLevel
    {
        private const long AccountId = 2;

        private const AccountLevel AccountLevel = EmployerAccounts.Models.Account.AccountLevel
            .AccountHasPAYEAndAllOrganisationsHaveSignedAgreements;

        private Mock<IEmployerAccountRepository> _employerAccountRepository;

        public GetAccountLevelQuery Query { get; set; }
        public GetAccountLevelQueryHandler RequestHandler { get; set; }
        public Mock<IValidator<GetAccountLevelQuery>> RequestValidator { get; set; }

        [SetUp]
        public void Arrange()
        {
            Query = new GetAccountLevelQuery()
            {
                AccountId = AccountId
            };

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();

            _employerAccountRepository.Setup(x => x.GetAccountLevel(AccountId)).ReturnsAsync(AccountLevel);

            RequestHandler = new GetAccountLevelQueryHandler(_employerAccountRepository.Object);
        }

        [Test]
        public async Task ThenTheRepositoryIsCalled()
        {
            await RequestHandler.Handle(new GetAccountLevelQuery { AccountId = AccountId });
            _employerAccountRepository.Verify(x => x.GetAccountLevel(AccountId));
        }

        [Test]
        public async Task ThenTheAccountLevelIsReturnedInTheResponse()
        {
            var result = await RequestHandler.Handle(new GetAccountLevelQuery { AccountId = AccountId });
            Assert.That(result.AccountLevel, Is.EqualTo(AccountLevel));
        }
    }
}
