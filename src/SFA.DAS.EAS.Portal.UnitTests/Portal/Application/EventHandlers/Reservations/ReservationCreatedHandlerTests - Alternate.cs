using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EAS.Portal.Application.EventHandlers.Reservations;
using SFA.DAS.EAS.Portal.UnitTests.Portal.Helpers;
using SFA.DAS.Reservations.Messages;

namespace SFA.DAS.EAS.Portal.UnitTests.Portal.Application.EventHandlers.Reservations
{
    [TestFixture, Parallelizable]
    public class ReservationCreatedHandlerTestsAlternate
    {
        private IFixture Fixture = new Fixture();
        private ReservationCreatedHandler _sut;
        private ReservationCreatedEvent _testEvent;
        private AccountDocServiceMockHelper _accountDocServiceMockHelper;

        [SetUp]
        public void SetUp()
        {
            _accountDocServiceMockHelper = new AccountDocServiceMockHelper();
            _sut = new ReservationCreatedHandler(_accountDocServiceMockHelper.AccountDocumentServiceMock.Object);
            _testEvent = Fixture.Create<ReservationCreatedEvent>();
        }

        [Test]
        public async Task Handle_WhenAccountDoesNotExist_ThenAccountDocumentIsSavedWithNewReservation()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpNoAccountDocument(_testEvent.AccountId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .SingleOrDefault(org => org.AccountLegalEntityId == _testEvent.AccountLegalEntityId).Reservations
                .Should()
                .ContainSingle(x => x.Id == _testEvent.Id);
        }

        [Test]
        public async Task Handle_WhenAccountDoesNotContainOrganisation_ThenAccountDocumentIsSavedWithNewReservation()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpEmptyAccountDoc(_testEvent.AccountId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .SingleOrDefault(org => org.AccountLegalEntityId == _testEvent.AccountLegalEntityId).Reservations
                .Should()
                .ContainSingle(x => x.Id == _testEvent.Id);
        }

        [Test]
        public async Task Handle_WhenAccountDoesContainOrganisationButNotReservation_ThenAccountDocumentIsSavedWithNewReservation()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpAccountWithOrg(_testEvent.AccountId, _testEvent.AccountLegalEntityId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .SingleOrDefault(org => org.AccountLegalEntityId == _testEvent.AccountLegalEntityId).Reservations
                .Should()
                .ContainSingle(x => x.Id == _testEvent.Id);
        }

        [Test]
        public void Handle_WhenAccountDoesContainOrganisationAndReservation_ThenExceptionIsThrown()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpAccountWithReservation(_testEvent.AccountId, _testEvent.AccountLegalEntityId, _testEvent.Id);

            // Act
            var exception = Assert.ThrowsAsync<Exception>(() => _sut.Handle(_testEvent));

            //Assert
            Assert.IsTrue(exception.Message.StartsWith("Received ReservationCreatedEvent with"));
        }
    }
}
