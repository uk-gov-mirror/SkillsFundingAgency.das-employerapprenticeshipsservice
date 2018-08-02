using System;
using System.Linq;
using System.Threading.Tasks;
using BoDi;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.Steps
{
    [Binding]
    public class FinanceSteps : TechTalk.SpecFlow.Steps
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;

        public FinanceSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;
        }

        [Then(@"user ([^ ]*) looking at account ([^ ]*) should see a balance of (.*) on the (.*)/(.*)")]
        [Then(@"user ([^ ]*) from account ([^ ]*) should see a level 1 screen with a balance of (.*) on the (.*)/(.*)")]
        public async Task ThenLevel1HasRowWithCorrectBalance(string username, string accountName, int balance, int month, int year)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var actual = await _objectContext.EatOrchestrator.GetAccountTransactions(account.HashedId, year, month, user.Ref.ToString());
            Assert.AreEqual(balance, actual.Data.Model.CurrentBalance);
        }

        [Then(@"user ([^ ]*) from account ([^ ]*) should see a level 1 screen with a total levy of (.*) on the (.*)/(.*)")]
        public async Task ThenLevel1HasRowWithCorrectTotalLevy(string username, string accountName, int totalLevy, int month, int year)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var actual = await _objectContext.EatOrchestrator.GetAccountTransactions(account.HashedId, year, month, user.Ref.ToString());
            Assert.AreEqual(totalLevy, actual.Data.Model.Data.TransactionLines.Sum(t => t.Amount));
        }

        [Then(@"user ([^ ]*) from account ([^ ]*) should see a level 2 screen with a levy declared of ([^ ]*) on the (.*)/(.*)")]
        public async Task ThenUserDaveFromAccountAShouldSeeALevelScreenWithALevyDeclaredOfOnThe(string username, string accountName, int levyDeclared, int month, int year)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var fromDate = new DateTime(year, month, 1);
            var toDate = new DateTime(year, month + 1, 1).AddMilliseconds(-1);

            var viewModel = await _objectContext.EatOrchestrator.FindAccountLevyDeclarationTransactions(account.HashedId, fromDate, toDate, user.Ref.ToString());

            Assert.AreEqual(levyDeclared, viewModel.Data.Amount - viewModel.Data.SubTransactions.Sum(x => x.TopUp));
        }

        [Then(@"user ([^ ]*) from account ([^ ]*) should see a level 2 screen with a top up of ([^ ]*) on the (.*)/(.*)")]
        public async Task ThenUserDaveFromAccountAShouldSeeALevelScreenWithATopUpOfOnThe(string username, string accountName, int topUp, int month, int year)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var fromDate = new DateTime(year, month, 1);
            var toDate = new DateTime(year, month + 1, 1).AddMilliseconds(-1);

            var viewModel = await _objectContext.EatOrchestrator.FindAccountLevyDeclarationTransactions(account.HashedId, fromDate, toDate, user.Ref.ToString());

            var topUpTotal = viewModel.Data.SubTransactions.Sum(x => x.TopUp);

            Assert.AreEqual(topUp, topUpTotal);
        }
    }
}
