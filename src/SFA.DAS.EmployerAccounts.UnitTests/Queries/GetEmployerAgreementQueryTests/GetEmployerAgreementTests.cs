namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAgreementQueryTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using AutoMapper;

    using FluentAssertions;

    using Moq;

    using NUnit.Framework;

    using SFA.DAS.EmployerAccounts.Data;
    using SFA.DAS.EmployerAccounts.Mappings;
    using SFA.DAS.EmployerAccounts.Models.Account;
    using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
    using SFA.DAS.EmployerAccounts.Models.UserProfile;
    using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement;
    using SFA.DAS.EmployerAccounts.TestCommon;
    using SFA.DAS.Validation;

    [TestFixture]
    class GetEmployerAgreementTests : Testing.FluentTest<GetEmployerAgreementTestFixtures>
    {
        const long AccountId = 1;

        const string HashedAccountId = "ACC123";

        const long LegalEntityId = 2;

        [Test]
        public Task GetAgreementToSign_IfAgreementHasNotBeenSigned_ShouldAddCurrentUserAsSigner()
        {
            User user = null;
            EmployerAgreement latestAgreement = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithPendingAgreement(AccountId, LegalEntityId, 2, out latestAgreement)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, latestAgreement.Id, user.Ref),
                assert: fixtures => Assert.AreEqual(user.FullName, fixtures.Response.EmployerAgreement.SignedByName));
        }

        [Test]
        public Task GetAgreementToSign_IfNoAgreementFound_ShouldReturn()
        {
            return this.TestAsync(
                act: fixtures => fixtures.Handle("ACC123", 1, Guid.NewGuid()),
                assert: fixtures =>
                    {
                        Assert.IsNull(fixtures.Response.EmployerAgreement);
                        Assert.IsNull(fixtures.Response.LastSignedAgreement);
                    });
        }

        [Test]
        public Task GetAgreementToSign_IfNoSignedAgreementsExists_ShouldReturnNoSignedAgreement()
        {
            User user = null;
            EmployerAgreement pendingAgreement = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithPendingAgreement(AccountId, LegalEntityId, 3, out pendingAgreement)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, pendingAgreement.Id, user.Ref),
                assert: fixtures => Assert.IsNull(fixtures.Response.LastSignedAgreement));
        }

        [Test]
        public Task GetAgreementToSign_IfRequestAgreementIsSigned_ShouldNotReturnLatestSignedAgreementAsWell()
        {
            User user = null;
            EmployerAgreement latestAgreement = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithSignedAgreement(AccountId, LegalEntityId, 3, DateTime.Now.AddDays(-10), out latestAgreement)
                    .WithSignedAgreement(AccountId, LegalEntityId, 2, DateTime.Now.AddDays(-20), out _)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, latestAgreement.Id, user.Ref),
                assert: fixtures => Assert.IsNull(fixtures.Response.LastSignedAgreement));
        }

        [Test]
        public Task GetAgreementToSign_IfRequestIsNotValid_DoNotShowAgreement()
        {
            User user = null;
            EmployerAgreement signedAgreement = null;

            return this.TestExceptionAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithSignedAgreement(AccountId, LegalEntityId, 3, DateTime.Now.AddDays(-20), out signedAgreement)
                    .WithUser(AccountId, "Buck", "Rogers", out user).WithInvalidRequest(),
                act: fixtures => fixtures.Handle(HashedAccountId, signedAgreement.Id, user.Ref),
                assert: (f, r) => r.ShouldThrowExactly<InvalidRequestException>());
        }

        [Test]
        public Task GetAgreementToSign_IfSignedAgreementIfForAnotherAccount_ShouldNotReturnSignedAgreement()
        {
            User user = null;
            EmployerAgreement latestAgreement = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithAccount(AccountId + 1, "XXX123")
                    .WithPendingAgreement(AccountId, LegalEntityId, 3, out latestAgreement)
                    .WithSignedAgreement(AccountId + 1, LegalEntityId, 2, DateTime.Now.AddDays(-30), out _)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, latestAgreement.Id, user.Ref),
                assert: fixtures => Assert.IsNull(fixtures.Response.LastSignedAgreement));
        }

        [Test]
        public Task GetAgreementToSign_IfUserIsNotAuthorized_DoNotShowAgreement()
        {
            User user = null;
            EmployerAgreement signedAgreement = null;

            return this.TestExceptionAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithSignedAgreement(AccountId, LegalEntityId, 3, DateTime.Now.AddDays(-20), out signedAgreement)
                    .WithUser(AccountId, "Buck", "Rogers", out user).WithCallerAsUnauthorizedUser(),
                act: fixtures => fixtures.Handle(HashedAccountId, signedAgreement.Id, user.Ref),
                assert: (fixturesf, r) => r.ShouldThrowExactly<UnauthorizedAccessException>());
        }

        [Test]
        public Task GetAgreementToSign_ShouldReturnLatestSignedAgreement()
        {
            EmployerAgreement latestSignedAgreement = null;
            EmployerAgreement pendingAgreement = null;
            User user = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithSignedAgreement(AccountId, LegalEntityId, 1, DateTime.Now.AddDays(-60), out _)
                    .WithSignedAgreement(
                        AccountId,
                        LegalEntityId,
                        2,
                        DateTime.Now.AddDays(-30),
                        out latestSignedAgreement)
                    .WithPendingAgreement(AccountId, LegalEntityId, 3, out pendingAgreement)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, pendingAgreement.Id, user.Ref),
                assert: fixtures =>
                    {
                        Assert.AreEqual(pendingAgreement.Id, fixtures.Response.EmployerAgreement.Id);
                        Assert.AreEqual(latestSignedAgreement.Id, fixtures.Response.LastSignedAgreement.Id);
                    });
        }

        [Test]
        public Task GetAgreementToSign_ShouldReturnRequestedAgreement()
        {
            EmployerAgreement expectedAgreement = null;
            User user = null;

            return this.TestAsync(
                arrange: fixtures => fixtures.WithAccount(AccountId, HashedAccountId)
                    .WithAgreement(AccountId, LegalEntityId, 1, EmployerAgreementStatus.Pending, out expectedAgreement)
                    .WithAgreement(AccountId, LegalEntityId, 2, EmployerAgreementStatus.Pending, out _)
                    .WithUser(AccountId, "Buck", "Rogers", out user),
                act: fixtures => fixtures.Handle(HashedAccountId, expectedAgreement.Id, user.Ref),
                assert: fixtures => Assert.AreEqual(expectedAgreement.Id, fixtures.Response.EmployerAgreement.Id));
        }
    }

    internal class GetEmployerAgreementTestFixtures
    {
        public GetEmployerAgreementTestFixtures()
        {
            this.EmployerAgreementBuilder = new EmployerAgreementBuilder();

            this.Validator = new Mock<IValidator<GetEmployerAgreementRequest>>();

            this.ConfigurationProvider = new MapperConfiguration(
                c =>
                    {
                        c.AddProfile<AccountMappings>();
                        c.AddProfile<AgreementMappings>();
                        c.AddProfile<EmploymentAgreementStatusMappings>();
                        c.AddProfile<LegalEntityMappings>();
                    });

            this.Validator.Setup(x => x.ValidateAsync(It.IsAny<GetEmployerAgreementRequest>()))
                .ReturnsAsync(new ValidationResult());
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public GetEmployerAgreementResponse Response { get; private set; }

        public Mock<IValidator<GetEmployerAgreementRequest>> Validator { get; }

        private EmployerAgreementBuilder EmployerAgreementBuilder { get; }

        public GetEmployerAgreementRequest BuildRequest(string hashedAccountId, long agreementId, Guid externalUserId)
        {
            var agreementHashId = this.EmployerAgreementBuilder.HashingService.HashValue(agreementId);
            var request = new GetEmployerAgreementRequest
                {
                    HashedAccountId = hashedAccountId,
                    AgreementId = agreementHashId,
                    ExternalUserId = externalUserId.ToString()
                };

            return request;
        }

        public void EvaluateSignedAndPendingAgreementIdsForAllAccountLegalEntities()
        {
            this.EmployerAgreementBuilder.EvaluateSignedAndPendingAgreementIdsForAllAccountLegalEntities();
        }

        public async Task Handle(string hashedAccountId, long agreementId, Guid externalUserId)
        {
            this.EmployerAgreementBuilder.EvaluateSignedAndPendingAgreementIdsForAllAccountLegalEntities();
            this.EmployerAgreementBuilder.SetupMockDbContext();
            var request = this.BuildRequest(hashedAccountId, agreementId, externalUserId);

            var handler = new GetEmployerAgreementQueryHandler(
                new Lazy<EmployerAccountsDbContext>(() => this.EmployerAgreementBuilder.EmployerAccountDbContext),
                this.EmployerAgreementBuilder.HashingService,
                this.Validator.Object,
                this.ConfigurationProvider);

            this.Response = await handler.Handle(request);
        }

        public GetEmployerAgreementTestFixtures WithAccount(long accountId, string hashedAccountId)
        {
            this.EmployerAgreementBuilder.WithAccount(accountId, hashedAccountId);
            return this;
        }

        public GetEmployerAgreementTestFixtures WithAgreement(
            long accountId,
            long legalEntityId,
            int agreementVersion,
            EmployerAgreementStatus status,
            out EmployerAgreement employerAgreement)
        {
            this.EmployerAgreementBuilder.WithAgreement(
                accountId,
                legalEntityId,
                agreementVersion,
                status,
                out employerAgreement);

            return this;
        }

        public GetEmployerAgreementTestFixtures WithCallerAsUnauthorizedUser()
        {
            this.Validator.Setup(x => x.ValidateAsync(It.IsAny<GetEmployerAgreementRequest>())).ReturnsAsync(
                new ValidationResult
                    {
                        ValidationDictionary = new Dictionary<string, string>(), IsUnauthorized = true
                    });

            return this;
        }

        public GetEmployerAgreementTestFixtures WithInvalidRequest()
        {
            this.Validator.Setup(x => x.ValidateAsync(It.IsAny<GetEmployerAgreementRequest>())).ReturnsAsync(
                new ValidationResult
                    {
                        ValidationDictionary = new Dictionary<string, string>
                            {
                                { "HashedAccountId", "Account Id is not a valid Id" }
                            },
                        IsUnauthorized = false
                    });

            return this;
        }

        public GetEmployerAgreementTestFixtures WithPendingAgreement(
            long accountId,
            long legalEntityId,
            int agreementVersion)
        {
            this.EmployerAgreementBuilder.WithPendingAgreement(accountId, legalEntityId, agreementVersion);
            return this;
        }

        public GetEmployerAgreementTestFixtures WithPendingAgreement(
            long accountId,
            long legalEntityId,
            int agreementVersion,
            out EmployerAgreement employerAgreement)
        {
            this.EmployerAgreementBuilder.WithPendingAgreement(
                accountId,
                legalEntityId,
                agreementVersion,
                out employerAgreement);
            return this;
        }

        public GetEmployerAgreementTestFixtures WithSignedAgreement(
            long accountId,
            long legalEntityId,
            int agreementVersion,
            DateTime signedTime,
            out EmployerAgreement employerAgreement)
        {
            this.EmployerAgreementBuilder.WithSignedAgreement(
                accountId,
                legalEntityId,
                agreementVersion,
                signedTime,
                out employerAgreement);
            return this;
        }

        public GetEmployerAgreementTestFixtures WithUser(
            long accountId,
            string firstName,
            string lastName,
            out User user)
        {
            var account = this.EmployerAgreementBuilder.GetAccount(accountId);
            this.EmployerAgreementBuilder.WithUser(account.Id, firstName, lastName, out user);
            return this;
        }
    }
}