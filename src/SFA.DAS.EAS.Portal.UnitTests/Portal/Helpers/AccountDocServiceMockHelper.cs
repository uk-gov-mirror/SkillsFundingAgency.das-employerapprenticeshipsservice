using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.EAS.Portal.Application.Services;
using SFA.DAS.EAS.Portal.Client.Database.Models;
using SFA.DAS.EAS.Portal.Client.Types;

namespace SFA.DAS.EAS.Portal.UnitTests.Portal.Helpers
{
    internal class AccountDocServiceMockHelper
    {
        private IFixture Fixture = new Fixture();
        internal Mock<IAccountDocumentService> AccountDocumentServiceMock { get; }
        internal AccountDocument SavedAccountDocument { get; private set; }

        public AccountDocServiceMockHelper()
        {
            Fixture.Customize<AccountDocument>(cus => cus.OmitAutoProperties());
            
            AccountDocumentServiceMock = new Mock<IAccountDocumentService>();

            AccountDocumentServiceMock
                .Setup(mock => mock.Save(It.IsAny<AccountDocument>(), It.IsAny<CancellationToken>()))
                .Callback((AccountDocument acc, CancellationToken cancellationToken) =>
                {
                    SavedAccountDocument = acc;
                })
                .Returns(Task.CompletedTask);
        }

        internal void SetUpNoAccountDocument(long accountId)
        {
            AccountDocumentServiceMock
                .Setup(mock => mock.GetOrCreate(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccountDocument(accountId));
        }


        internal void SetUpEmptyAccountDoc(long accountId)
        {
            var accountDocument = JsonConvert.DeserializeObject<AccountDocument>($"{{\"Account\": {{\"Id\": {accountId} }}}}");

            AccountDocumentServiceMock
                .Setup(mock => mock.GetOrCreate(accountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDocument);
        }

        internal void SetUpAccountWithOrg(long accountId, long accountLegalEntityId)
        {
            var accountDocument = Fixture
                .Build<AccountDocument>()
                .With(ad => ad.Account, Fixture.Build<Account>().With(acc => acc.Id, accountId).Create())
                .Create();

            var organisation = accountDocument.Account.Organisations.RandomElement();
            organisation.AccountLegalEntityId = accountLegalEntityId;
            organisation.Reservations = new List<Reservation>();

            AccountDocumentServiceMock.Setup(s => s.GetOrCreate(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(accountDocument);
        }

        internal void SetUpAccountWithReservation(long accountId, long accountLegalEntityId, Guid reservationId)
        {
            var accountDocument = Fixture
                .Build<AccountDocument>()
                .With(ad => ad.Account, Fixture.Build<Account>().With(acc => acc.Id, accountId).Create())
                .Create();

            var organisation = accountDocument.Account.Organisations.RandomElement();
            organisation.AccountLegalEntityId = accountLegalEntityId;
            var reservation = organisation.Reservations.RandomElement();
            reservation.Id = reservationId;

            AccountDocumentServiceMock.Setup(s => s.GetOrCreate(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(accountDocument);
        }
    }
}
