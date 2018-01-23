using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Api;
using SFA.DAS.EAS.Api.DependencyResolution;
using SFA.DAS.EAS.Domain.Data.Entities.Account;
using SFA.DAS.EAS.Domain.Data.Repositories;

namespace SFA.DAS.EAS.Account.Api.Tests
{
    [TestFixture]
    public class Class1
    {
        private TestServer _server;
        private IAccountApiClient _client;
        private Mock<IEmployerAccountRepository> mockRepo;
        private Mock<IDasLevyRepository> mockLevyRepo;

        [Test]
        public async Task ShouldRetreiveAnAccountWithBalane()
        {
            var account = new Domain.Data.Entities.Account.Account { HashedId = "KAKAKA", Id = 123, Name = "Test", RoleId = 1};
            var accounts = new Accounts<Domain.Data.Entities.Account.Account>{AccountList = new List<Domain.Data.Entities.Account.Account>()
            {
                account
            }};

            var balance = new AccountBalance {AccountId = 123, Balance = 100.0m, IsLevyPayer = 1};
            var balances = new List<AccountBalance>()
            {
                balance
            };

            mockRepo.Setup(x => x.GetAccounts(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(accounts));
            mockLevyRepo.Setup(x => x.GetAccountBalances(It.IsAny<List<long>>())).Returns(Task.FromResult(balances));

            var container = IocConfig.Container;
            container.EjectAllInstancesOf<IEmployerAccountRepository>();
            container.Configure(x =>
            {
                x.For<IEmployerAccountRepository>().ClearAll().Use(mockRepo.Object);
                x.For<IDasLevyRepository>().ClearAll().Use(mockLevyRepo.Object);
            });

            var pageOfAccounts = await _client.GetPageOfAccounts(1, 10, null);
            Console.WriteLine(JsonConvert.SerializeObject(pageOfAccounts));

            Assert.AreEqual(1, pageOfAccounts.Data.Count);
            Assert.AreEqual(balance.IsLevyPayer == 1, pageOfAccounts.Data[0].IsLevyPayer);
            Assert.AreEqual(account.HashedId, pageOfAccounts.Data[0].AccountHashId);
            Assert.AreEqual(account.Id, pageOfAccounts.Data[0].AccountId);
            Assert.AreEqual(balance.Balance, pageOfAccounts.Data[0].Balance);
        }

        [SetUp]
        public void Setup()
        {
            var process = Process.Start(@"C:\Program Files\Microsoft SDKs\Azure\Emulator\csrun", "/devstore");
            if (process != null)
            {
                process.WaitForExit();
            }
            else
            {
                throw new Exception("Unable to start storage emulator.");
            }
            _server = TestServer.Create<Startup>();
            _client = new AccountApiClient2(_server.HttpClient);

            mockRepo = new Mock<IEmployerAccountRepository>();
            mockLevyRepo = new Mock<IDasLevyRepository>();
        }

        [TearDown]
        public void TearDown()
        {
            _server.Dispose();
        }
    }
}
