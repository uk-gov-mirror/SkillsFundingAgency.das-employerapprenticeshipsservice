using System.Threading.Tasks;
using System.Web.Mvc;
using BoDi;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Data;
using SFA.DAS.EAS.TestCommon.Extensions;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Types;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.Steps
{
    [Binding]
    public class PayeSchemeSteps
    {
        private const string PayeSchemeAddedEventName = "PayeSchemeAddedEvent";
        private const string PayeSchemeRemovedEventName = "PayeSchemeRemovedEvent";

        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;
        private ActionResult _actionResult;

        public PayeSchemeSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;
        }

        [Given(@"user ([^ ]*) adds paye scheme ""([^ ]*)"" to account ([^ ]*)")]
        [When(@"user ([^ ]*) adds paye scheme ""([^ ]*)"" to account ([^ ]*)")]
        public async Task WhenUserUAddsPayeSchemePToAccountA(string username, string payeSchemeRef, string accountName)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var addPayeSchemeVm = new AddNewPayeSchemeViewModel
            {
                HashedAccountId = account.HashedId,
                AccessToken = "AccessToken",
                PayeName = "Name",
                PayeScheme = payeSchemeRef,
                UserId = user.Id,
                PublicHashedAccountId = account.PublicHashedId,
                RefreshToken = "RefreshToken"
            };

            user.SetMockAuthenticationServiceForUser(_objectContext.AuthenticationService);

            _actionResult = await _objectContext.EapController.ConfirmPayeScheme(account.HashedId, addPayeSchemeVm);
        }

        [When(@"user ([^ ]*) removes paye scheme ""(.*)"" from account ([^ ]*)")]
        public async Task WhenUserDaveRemovesPayeSchemeFromAccountA(string username, string payeSchemeRef, string accountName)
        {
            var user = _objectContext.Users[username];
            var account = _objectContext.Accounts[accountName];

            var removePayeSchemeVm = new RemovePayeSchemeViewModel()
            {
                HashedAccountId = account.HashedId,
                UserId = user.Id.ToString(),
                PublicHashedAccountId = account.PublicHashedId,
                PayeRef = payeSchemeRef,
                RemoveScheme = 2
            };

            user.SetMockAuthenticationServiceForUser(_objectContext.AuthenticationService);
            _actionResult = await _objectContext.EapController.RemovePaye(account.HashedId, removePayeSchemeVm);
        }


        [Then(@"The an active PAYE scheme ""(.*)"" is added to account ([^ ] *)")]
        public async Task ThenAnActivePayeSchemePIsAddedToAccountA(string payeSchemeRef, string accountName)
        {
            var account = _objectContext.Accounts[accountName];

            var repo = _objectContainer.Resolve<IPayeRepository>();
            var payeScheme = await repo.GetPayeForAccountByRef(account.HashedId, payeSchemeRef);

            Assert.IsNotNull(payeScheme);
            Assert.IsNull(payeScheme.RemovedDate);
        }

        [Then(@"The an active PAYE scheme ""(.*)"" is removed from account ([^ ] *)")]
        public async Task ThenTheAnActivePayeSchemeIsRemovedFromAccountA(string payeSchemeRef, string accountName)
        {
            var account = _objectContext.Accounts[accountName];

            var repo = _objectContainer.Resolve<IPayeRepository>();
            var payeScheme = await repo.GetPayeForAccountByRef(account.HashedId, payeSchemeRef);

            Assert.IsNotNull(payeScheme.RemovedDate);
        }


        [Then(@"The user is redirected to the next steps view")]
        public void ThenTheUserIsRedirectedToTheNextStepsView()
        {
            var redirect = (RedirectToRouteResult)_actionResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual(ControllerConstants.NextStepsActionName, redirect.RouteValues["action"]);
            Assert.AreEqual(ControllerConstants.EmployerAccountPayeControllerName, redirect.RouteValues["controller"]);
        }

        [Then(@"The user is redirected to the paye index view")]
        public void ThenTheUserIsRedirectedToThePayeIndexView()
        {
            var redirect = (RedirectToRouteResult)_actionResult;

            Assert.IsNotNull(redirect);
            Assert.AreEqual(ControllerConstants.IndexActionName, redirect.RouteValues["action"]);
            Assert.AreEqual(ControllerConstants.EmployerAccountPayeControllerName, redirect.RouteValues["controller"]);
        }

        [Then(@"A Paye Scheme Added event is raised")]
        public void ThenAPayeSchemeAddedEventIsRaised()
        {
            _objectContext.EventsApiMock.Verify(e =>
                e.CreateGenericEvent(It.Is<GenericEvent>(ev => ev.Type == PayeSchemeAddedEventName)));
        }

        [Then(@"A Paye Scheme Removed event is raised")]
        public void ThenAPayeSchemeRemovedEventIsRaised()
        {
            _objectContext.EventsApiMock.Verify(e =>
                e.CreateGenericEvent(It.Is<GenericEvent>(ev => ev.Type == PayeSchemeRemovedEventName)));
        }
    }
}