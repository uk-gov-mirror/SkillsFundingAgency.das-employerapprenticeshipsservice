using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.Models;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests
{
    public class WhenINavigateToConfirmWhoYouAreWithAReturnUrl
    {
        private Mock<IAuthenticationService> _owinWrapper;
        private HomeController _homeController;
        private EmployerAccountsConfiguration _configuration;
        private string ExpectedUserId = "123ABC";
        private Mock<ICookieStorageService<ReturnUrlModel>> _returnUrlCookieStorage;
        private string ExpectedReturnUrl = "test.com";
        private string ReturnUrlCookieName = "SFA.DAS.EmployerAccounts.Web.Controllers.ReturnUrlCookie";

        [SetUp]
        public void Arrange()
        {
            _owinWrapper = new Mock<IAuthenticationService>();
            _configuration = new EmployerAccountsConfiguration();
            _returnUrlCookieStorage = new Mock<ICookieStorageService<ReturnUrlModel>>();

            _homeController = new HomeController(
                _owinWrapper.Object, Mock.Of<HomeOrchestrator>(), _configuration, Mock.Of<IAuthorizationService>(),
                Mock.Of<IMultiVariantTestingService>(), Mock.Of<ICookieStorageService<FlashMessageViewModel>>(), _returnUrlCookieStorage.Object);
        }


        [Test]
        public async Task ThenTheClaimsAreRefreshedForThatUserWhenAuthenticated()
        {
            //Arrange
            //Act
            _homeController.ConfirmWhoYouAre(ExpectedReturnUrl);

            //Assert
            _returnUrlCookieStorage.Verify(x => x.Create(It.Is<ReturnUrlModel>(y => y.Value == ExpectedReturnUrl), ReturnUrlCookieName, 1));
        }
    }
}
