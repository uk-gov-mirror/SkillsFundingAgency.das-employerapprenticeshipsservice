using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Portal.Application.Services;
using SFA.DAS.EAS.Portal.Client.Database.Models;
using SFA.DAS.EAS.Portal.Client.Types;
using SFA.DAS.Testing;
using System;
using System.Threading.Tasks;
using Fix = SFA.DAS.EAS.Portal.UnitTests.Portal.Application.EventHandlers.Commitments.CohortApprovalRequestedByProviderHandlerTestsFixture;
using FluentAssertions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.EAS.Portal.Application.EventHandlers.Cohort;
using SFA.DAS.HashingService;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types.Commitment;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using SFA.DAS.EAS.Portal.UnitTests.Portal.Helpers;

namespace SFA.DAS.EAS.Portal.UnitTests.Portal.Application.EventHandlers.Commitments
{
    [TestFixture, Parallelizable]
    public class CohortApprovalRequestedHandlerTestsAlternate
    {
        private IFixture Fixture = new Fixture();
        private CohortApprovalRequestedByProviderHandler _sut;
        private CohortApprovalRequestedByProvider _testEvent;
        private AccountDocServiceMockHelper _accountDocServiceMockHelper;
        private Mock<IProviderCommitmentsApi> _providerCommitmentsApiMock;
        private Mock<IHashingService> _hashingServiceMock;
        private long _accountLegalEntityId = 1;

        [SetUp]
        public void SetUp()
        {
            _providerCommitmentsApiMock = new Mock<IProviderCommitmentsApi>();
            _hashingServiceMock = new Mock<IHashingService>();
            _accountDocServiceMockHelper = new AccountDocServiceMockHelper();
            _sut = new CohortApprovalRequestedByProviderHandler(
                _accountDocServiceMockHelper.AccountDocumentServiceMock.Object, 
                _providerCommitmentsApiMock.Object, 
                _hashingServiceMock.Object);
            _testEvent = Fixture.Create<CohortApprovalRequestedByProvider>();

            var commitment = Fixture.Create<CommitmentView>();

            _providerCommitmentsApiMock
                .Setup(m => m.GetProviderCommitment(_testEvent.ProviderId, _testEvent.CommitmentId))
                .ReturnsAsync(commitment);

            _hashingServiceMock
                .Setup(m => m.DecodeValue(commitment.AccountLegalEntityPublicHashedId))
                .Returns(_accountLegalEntityId);
        }

        [Test]
        public async Task Handle_WhenAccountDoesNotExist_ThenAccountDocumentIsSavedWithNewCohort()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpNoAccountDocument(_testEvent.AccountId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            AssertAccountAsExpected(1);
        }

        [Test]
        public async Task Handle_WhenAccountDoesNotContainOrganisation_ThenAccountDocumentIsSavedWithNewCohort()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpEmptyAccountDoc(_testEvent.AccountId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            AssertAccountAsExpected(1);
        }

        [Test]
        public async Task Handle_WhenAccountDoesContainOrganisationButNotCohort_ThenAccountDocumentIsSavedWithNewCohort()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpAccountWithOrg(_testEvent.AccountId, _accountLegalEntityId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            AssertAccountAsExpected(3);
        }

        [Test]
        public async Task Handle_WhenAccountDoesContainOrganisationAndCohort_ThenAccountDocumentIsSavedWithUpdatedCohort()
        {
            // Arrange
            _accountDocServiceMockHelper.SetUpAccountWithCohort(_testEvent.AccountId, _accountLegalEntityId, _testEvent.CommitmentId);

            // Act 
            await _sut.Handle(_testEvent);

            //Assert
            AssertAccountAsExpected(3);
            AssertCohortUpdated();
        }

        [Test]
        public void Execute_WhenProviderCommitmentApiThrows_ThenExceptionIsPropagated()
        {
            // Arrange
            _providerCommitmentsApiMock.Setup(mock => mock.GetProviderCommitment(It.IsAny<long>(), It.IsAny<long>())).Throws(new Exception("Broke"));

            // Act
            var exception = Assert.ThrowsAsync<Exception>(() => _sut.Handle(_testEvent));

            //Assert
            Assert.IsTrue(exception.Message.StartsWith("Broke"));
        }

        private void AssertAccountAsExpected(int expectedOrgCount)
        {
            _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .Should()
                .HaveCount(expectedOrgCount);

            _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .SingleOrDefault(org => org.AccountLegalEntityId == _accountLegalEntityId).Cohorts
                .Should()
                .ContainSingle(x => x.Id == _testEvent.CommitmentId.ToString());
        }

        private void AssertCohortUpdated()
        {
            var cohort = _accountDocServiceMockHelper
                .SavedAccountDocument
                .Account
                .Organisations
                .SingleOrDefault(org => org.AccountLegalEntityId == _accountLegalEntityId)
                .Cohorts
                .SingleOrDefault(coh => coh.Id == _testEvent.CommitmentId.ToString());

            cohort.Apprenticeships.Should().HaveCountGreaterThan(0);
        }
    }
}
