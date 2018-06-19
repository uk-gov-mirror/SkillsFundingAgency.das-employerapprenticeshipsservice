using System.Threading.Tasks;
using BoDi;
using MediatR;
using Moq;
using SFA.DAS.EAS.Application.Commands.Payments.RefreshPaymentData;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.Provider.Events.Api.Client;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Finance.AcceptanceTests.Steps
{
    [Binding]
    public class PaymentSteps
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;
        private readonly Mock<IPaymentsEventsApiClient> _paymentsApiClient;

        public PaymentSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;
            _paymentsApiClient = new Mock<IPaymentsEventsApiClient>();

            var container = objectContainer.Resolve<IContainer>();

            container.Inject(_paymentsApiClient.Object);
        }

        [Given(@"account ([^ ]*) makes a payment for standard ([^ ]*) and period end ([^ ]*)")]
        public void WhenIMakeAPaymentForTheApprenticeshipStandard(string accountName, string standardName, string periodEndId)
        {
            var account = _objectContext.Accounts[accountName];

            _paymentsApiClient.Setup(c => c.GetPayments(periodEndId, account.Id.ToString(), 1, null))
                .ReturnsAsync(null);
        }

        [When(@"account ([^ ]*) payments are imported for period end ([^ ]*)")]
        public Task WhenAccountPaymentsAreImported(string accountName, string periodEndId)
        {
            var account = _objectContext.Accounts[accountName];
            var mediator = _objectContainer.Resolve<IMediator>();

            return mediator.SendAsync(new RefreshPaymentDataCommand
            {
                AccountId = account.Id,
                PeriodEnd = periodEndId,
                PaymentUrl = "/"
            });
        }
    }
}