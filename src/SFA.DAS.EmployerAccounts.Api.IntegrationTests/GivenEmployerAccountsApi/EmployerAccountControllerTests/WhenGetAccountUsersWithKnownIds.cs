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
    public class WhenGetAccountUsersWithKnownIds
    :GivenEmployerAccountsApi
    {
        [Test]
        public void ThenCorrectTeamMembersAreReturned()
        {
            Response
                .ShouldHaveContentOfType<List<TeamMember>>();

            var teamMembers = Response.GetContent<List<TeamMember>>();

            teamMembers
                .Count
                .Should()
                .Be(1);

            var firstTeamMember = teamMembers[0];

            firstTeamMember
                .Role
                .Should()
                .Be("Owner");

            firstTeamMember
                .Name
                .Should()
                .Be("Test Account");
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
            return @"https://localhost:44330/api/accounts/84VBNV/users";
        }
    }
}