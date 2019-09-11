﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EAS.Account.API.IntegrationTests.TestUtils.DataAccess.DataHelpers;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.ModelBuilders;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.TestUtils.DataAccess;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.TestUtils.DataAccess.Dtos;
using SFA.DAS.EmployerAccounts.Api.Types;

namespace SFA.DAS.EmployerAccounts.Api.IntegrationTests.GivenEmployerAccountsApi.LegalEntitiesControllerTests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class WhenIGetASingleLegalEntityWithKnownIds
    :GivenEmployerAccountsApi
    {
        private EmployerAccountOutput _employerAccount;

        [SetUp]
        public async Task Setup()
        {
            await InitialiseEmployerAccountData(async builder =>
            {
                var data = new TestModelBuilder()
                    .WithNewUser()
                    .WithNewAccount();

                await builder.SetupDataAsync(data);

                _employerAccount = data.CurrentAccount.AccountOutput;
            });

            WhenControllerActionIsCalled( $"https://localhost:44330/api/accounts/{_employerAccount.HashedAccountId}/legalentities");
        }

        [Test]
        public void ThenTheStatusShouldBeFound_ByHashedAccountId()
        {
            var resources =
                Response
                    .GetContent<ResourceList>();

            Assert.IsNotNull(resources);
            Assert.AreEqual(1, resources.Count);
        }
    }
}