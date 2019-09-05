using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.Helpers;
using SFA.DAS.EmployerAccounts.Api.Types;

namespace SFA.DAS.EmployerAccounts.Api.IntegrationTests.GivenEmployerAccountsApi.EmployerAccountControllerTests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class WhenGetAccountUsersWithUnknownIds
    :GivenEmployerAccountsApi
    {
        [Test]
        public void ThenEmptyTeamMembersCollectionIsReturned()
        {
            Response
                .ShouldHaveContentOfType<List<TeamMember>>();

            Response.GetContent<List<TeamMember>>()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void ThenOkResponseIsReturn()
        {
            Response
                .StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }
        protected override string GetRequestUri()
        {
            return @"https://localhost:44330/api/accounts/MADE*UP*ID/users";
        }
    }
}