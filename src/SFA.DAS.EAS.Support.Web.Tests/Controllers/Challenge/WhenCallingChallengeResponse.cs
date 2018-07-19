using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.Infrastructure.Models;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Tests.Controllers.Challenge
{
    [TestFixture]
    public class WhenCallingChallengeResponse : WhenTestingChallengeController
    {
        /// <summary>
        ///     Note that this Controler method scenario sets HttpResponse.StatusCode = 403 (Forbidden), this result is not
        ///     testable from a unit test
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ItShouldReturnAViewModelWhenTheChallengeEntryIsInvalid()
        {
            PayeSchemeChallengeViewModel = new PayeSchemeChallengeViewModel
            {
                ChallengeId = Guid.NewGuid(),
                Balance = "£1000",
                Challenge1 = "1",
                Challenge2 = "A",
                FirstCharacterPosition = 0,
                SecondCharacterPosition = 1,
                ResponseUrl = "https://tempuri.org/challenge/response",
                ReturnTo = "https://tempuri.org/challenge/me/to/a/deul/any/time"
            };

            var query = new ChallengePermissionQuery
            {
                Id = "123",
                Balance = "£1000",
                ChallengeElement1 = "1",
                ChallengeElement2 = "A",
                FirstCharacterPosition = 1,
                SecondCharacterPosition = 2,
                Url = "https://tempuri.org/challenge/me/to/a/deul/any/time"
            };

            var response = new ChallengePermissionResponse
            {
                Id = PayeSchemeChallengeViewModel.Identifier,
                Url = PayeSchemeChallengeViewModel.ReturnTo,
                IsValid = false
            };

            MockChallengeHandler.Setup(x => x.Handle(It.IsAny<ChallengePermissionQuery>()))
                .ReturnsAsync(response);

            var actual = await Unit.Response(PayeSchemeChallengeViewModel);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<ViewResult>(actual);
            Assert.IsInstanceOf<PayeSchemeChallengeViewModel>(((ViewResult) actual).Model);
            Assert.AreEqual(true, ((PayeSchemeChallengeViewModel) ((ViewResult) actual).Model).HasError);
        }

        [Test]
        public async Task ItShouldReturnARedirectToTheReturnToAddressWhenTheChallengeEntryIsValid()
        {
            PayeSchemeChallengeViewModel = new PayeSchemeChallengeViewModel
            {
                ChallengeId = Guid.NewGuid(),
                Balance = "£1000",
                Challenge1 = "1",
                Challenge2 = "A",
                FirstCharacterPosition = 1,
                SecondCharacterPosition = 4,
                ReturnTo = "https://tempuri.org/challenge/me/to/a/deul/any/time"
            };

            var query = new ChallengePermissionQuery
            {
                Id = "123",
                Balance = "£1000",
                ChallengeElement1 = "1",
                ChallengeElement2 = "B",
                FirstCharacterPosition = 1,
                SecondCharacterPosition = 4,
                Url = "https://tempuri.org/challenge/me/to/a/deul/any/time"
            };

            var response = new ChallengePermissionResponse
            {
                Id = PayeSchemeChallengeViewModel.Identifier,
                Url = PayeSchemeChallengeViewModel.ReturnTo,
                IsValid = true
            };

            MockChallengeHandler.Setup(x => x.Handle(It.IsAny<ChallengePermissionQuery>()))
                .ReturnsAsync(response);

            var actual = await Unit.Response(PayeSchemeChallengeViewModel);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<RedirectResult>(actual);
            Assert.AreEqual(PayeSchemeChallengeViewModel.ReturnTo, ((RedirectResult)actual).Url);
        }
    }
}