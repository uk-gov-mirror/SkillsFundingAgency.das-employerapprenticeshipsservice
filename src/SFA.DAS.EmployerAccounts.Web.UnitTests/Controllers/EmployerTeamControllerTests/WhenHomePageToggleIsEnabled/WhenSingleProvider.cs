﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EAS.Portal.Client;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.FeatureToggles;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using System.Web.Mvc;
using SFA.DAS.Authorization.Services;
using Model = SFA.DAS.EAS.Portal.Client.Types;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests.WhenHomePageToggleIsEnabled
{
    public class WhenSingleProvider
    {
        private EmployerTeamController _controller;
        private Mock<IAuthorizationService> mockAuthorizationService;
        private Mock<IAuthenticationService> mockAuthenticationService;
        private Mock<IMultiVariantTestingService> mockMultiVariantTestingService;
        private Mock<ICookieStorageService<FlashMessageViewModel>> mockCookieStorageService;
        private Mock<EmployerTeamOrchestrator> mockEmployerTeamOrchestrator;
        private Mock<IPortalClient> mockPortalClient;

        [SetUp]
        public void Arrange()
        {
            mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthenticationService = new Mock<IAuthenticationService>();
            mockMultiVariantTestingService = new Mock<IMultiVariantTestingService>();
            mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
            mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestrator>();
            mockPortalClient = new Mock<IPortalClient>();

            mockAuthorizationService.Setup(m => m.IsAuthorized(It.Is<string>(s => s == "EmployerFeature.HomePage"))).Returns(true);

            _controller = new EmployerTeamController(
                mockAuthenticationService.Object,
                mockMultiVariantTestingService.Object,
                mockCookieStorageService.Object,
                mockEmployerTeamOrchestrator.Object,
                mockPortalClient.Object, 
                mockAuthorizationService.Object);
        }

        [Test]
        public void ThenTheSingleProviderViewIsReturned()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.AgreementsToSign = false;

            model.AccountViewModel = new Model.Account();
            model.AccountViewModel.Providers.Add(new Model.Provider());

            //Act
            var result = _controller.Row1Panel2(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("SingleProvider", (result.Model as dynamic).ViewName);
        }
    }
}