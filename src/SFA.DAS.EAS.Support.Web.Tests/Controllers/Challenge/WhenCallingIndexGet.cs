using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Tests.Controllers.Challenge
{
    [TestFixture]
    public class WhenCallingIndexGet : WhenTestingChallengeController
    {
        [Test]
        public async Task ItShouldReturnNoFoundViewWhenThereIsNotAMatch()
        {
            var challengeResponse = new ChallengeResponse
            {
                Account = null,
                StatusCode = SearchResponseCodes.NoSearchResultsFound
            };

            var id = Guid.NewGuid();

            MockChallengeHandler.Setup(x => x.Get(id.ToString()))
                .ReturnsAsync(challengeResponse);

            var actual = await Unit.Challenge(id);

            Assert.IsInstanceOf<object>(actual);
            
        }

        [Test]
        public async Task ItShouldReturnHttpNoFoundWhenTheSearchFails()
        {
            var challengeResponse = new ChallengeResponse
            {
                Account = null,
                StatusCode = SearchResponseCodes.SearchFailed
            };

            var id = Guid.NewGuid();

            MockChallengeHandler.Setup(x => x.Get(id.ToString()))
                .ReturnsAsync(challengeResponse);

            var actual = await Unit.Challenge(id);

            Assert.IsInstanceOf<HttpNotFoundResult>(actual);
        }

        [Test]
        public async Task ItShouldReturnTheChallengeViewWithAModelWhenThereIsAMatch()
        {
            var challengeResponse = new ChallengeResponse
            {
                Account = new Core.Models.Account
                {
                    AccountId = 123,
                    HashedAccountId = "ERERER",
                    DasAccountName = "Test Account"
                },
                StatusCode = SearchResponseCodes.Success
            };

            var id = Guid.NewGuid();

            MockChallengeHandler.Setup(x => x.Get(id.ToString()))
                .ReturnsAsync(challengeResponse);

            var actual = await Unit.Challenge(id);

            Assert.IsInstanceOf<ViewResult>(actual);
            var viewResult = (ViewResult) actual;
            Assert.IsInstanceOf<PayeSchemeChallengeViewModel>(viewResult.Model);
        }
    }
}