using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EmployerAccounts.Authorization;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.Models;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests
{
    public class WhenIHandleNewRegistration
    {
        private Mock<IAuthenticationService> _owinWrapper;
        private HomeController _homeController;
        private EmployerAccountsConfiguration _configuration;
        private string ExpectedUserId = "123ABC";

        [SetUp]
        public void Arrange()
        {
            _owinWrapper = new Mock<IAuthenticationService>();
            _configuration = new EmployerAccountsConfiguration();

            _homeController = new HomeController(
                _owinWrapper.Object, Mock.Of<HomeOrchestrator>(), _configuration, Mock.Of<IAuthorizationService>(),
                Mock.Of<IMultiVariantTestingService>(), Mock.Of<ICookieStorageService<FlashMessageViewModel>>(), Mock.Of<ICookieStorageService<ReturnUrlModel>>());
        }


        [Test]
        public async Task ThenTheClaimsAreRefreshedForThatUserWhenAuthenticated()
        {
            //Arrange
            _owinWrapper.Setup(x => x.GetClaimValue("sub")).Returns(ExpectedUserId);

            //Act
            await _homeController.HandleNewRegistration();

            //Assert
            _owinWrapper.Verify(x => x.UpdateClaims(), Times.Once);
        }
    }

    public class WhenIHandleNewRegistrationAndSelectLater
    {
        private Mock<IAuthenticationService> _owinWrapper;
        private HomeController _homeController;
        private EmployerAccountsConfiguration _configuration;
        private string ExpectedUserId = "123ABC";
        private Mock<ICookieStorageService<ReturnUrlModel>> _returnUrlCookieStorage;
        private string ExpectedReturnUrl = "test.com";

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
            _owinWrapper.Setup(x => x.GetClaimValue("sub")).Returns(ExpectedUserId);

            //Act
            await _homeController.HandleNewRegistration("later");

            //Assert
            _owinWrapper.Verify(x => x.UpdateClaims(), Times.Once);
        }

        [Test]
        public async Task ThenThePageRedirectsToYouHaveRegisteredIfThereIsNoReturnUrlCookie()
        {
            //Arrange
            //Act
            var actual = await _homeController.HandleNewRegistration("later");

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<RedirectToRouteResult>(actual);
            var actualRedirect = actual as RedirectToRouteResult;
            Assert.IsNotNull(actualRedirect);
            Assert.AreEqual("YouHaveRegistered", actualRedirect.RouteValues["action"]);
        }

        [Test]
        public async Task ThenThePageRedirectsToReturnUrlCookie()
        {
            //Arrange
            _returnUrlCookieStorage.Setup(x => x.Get("SFA.DAS.EmployerAccounts.Web.Controllers.ReturnUrlCookie"))
                .Returns(new ReturnUrlModel() {Value = ExpectedReturnUrl });

            //Act
            var actual = await _homeController.HandleNewRegistration("later");

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<RedirectResult>(actual);
            var actualRedirect = actual as RedirectResult;
            Assert.IsNotNull(actualRedirect);
            Assert.AreEqual(ExpectedReturnUrl, actualRedirect.Url);
        }


    }
}
