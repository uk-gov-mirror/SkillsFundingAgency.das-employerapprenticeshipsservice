using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Tests.Controllers.Account
{
    [TestFixture]
    public class WhenTestingIndexGet : WhenTestingAccountController
    {
        [Test]
        public async Task ItShouldReturnAViewAndModelOnSuccess()
        {
            var reponse = new AccountDetailOrganisationsResponse
            {
                Account = new Core.Models.Account
                {
                    AccountId = 123,
                    DasAccountName = "Test Account",
                    DateRegistered = DateTime.Today,
                    OwnerEmail = "owner@tempuri.org"
                },
                StatusCode = SearchResponseCodes.Success
            };
            var id = "123";
            AccountHandler.Setup(x => x.FindOrganisations(id)).ReturnsAsync(reponse);
            var actual = await Unit.Index("123");

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<ViewResult>(actual);
            Assert.AreEqual("", ((ViewResult)actual).ViewName);
            Assert.IsInstanceOf<AccountDetailViewModel>(((ViewResult)actual).Model);
            Assert.AreEqual(reponse.Account, ((AccountDetailViewModel)((ViewResult)actual).Model).Account);
            Assert.IsNull(((AccountDetailViewModel)((ViewResult)actual).Model).SearchUrl);
        }

        [Test]
        public async Task ItShouodReturnHttpNotFoundViewOnNoSearchResultsFound()
        {
            var reponse = new AccountDetailOrganisationsResponse
            {
                Account = new Core.Models.Account
                {
                    AccountId = 123,
                    DasAccountName = "Test Account",
                    DateRegistered = DateTime.Today,
                    OwnerEmail = "owner@tempuri.org"
                },
                StatusCode = SearchResponseCodes.NoSearchResultsFound
            };
            var id = "123";
            AccountHandler.Setup(x => x.FindOrganisations(id)).ReturnsAsync(reponse);
            var actual = await Unit.Index("123");
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<ViewResult>(actual);
            Assert.AreEqual("_notFound", ((ViewResult)actual).ViewName);
        }

        [Test]
        public async Task ItShouldReturnHttpNotFoundViewOnSearchFailed()
        {
            var reponse = new AccountDetailOrganisationsResponse
            {
                Account = new Core.Models.Account
                {
                    AccountId = 123,
                    DasAccountName = "Test Account",
                    DateRegistered = DateTime.Today,
                    OwnerEmail = "owner@tempuri.org"
                },
                StatusCode = SearchResponseCodes.SearchFailed
            };
            var id = "123";
            AccountHandler.Setup(x => x.FindOrganisations(id)).ReturnsAsync(reponse);
            var actual = await Unit.Index("123");

            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<ViewResult>(actual);
            Assert.AreEqual("_notFound", ((ViewResult)actual).ViewName);
        }
    }
}