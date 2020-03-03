﻿using System.Collections.Generic;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Reservations;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests.WhenCallToActionToggleIsEnabled
{
    public class WhenFundsToReserve
    {
        private EmployerTeamController _controller;

        private Mock<IAuthenticationService> mockAuthenticationService;
        private Mock<IAuthorizationService> mockAuthorizationService;
        private Mock<IMultiVariantTestingService> mockMultiVariantTestingService;
        private Mock<ICookieStorageService<FlashMessageViewModel>> mockCookieStorageService;
        private Mock<EmployerTeamOrchestrator> mockEmployerTeamOrchestrator;

        [SetUp]
        public void Arrange()
        {
            mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthorizationService = new Mock<IAuthorizationService>();
            mockMultiVariantTestingService = new Mock<IMultiVariantTestingService>();
            mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
            mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestrator>();

            mockAuthorizationService.Setup(m => m.IsAuthorized("EmployerFeature.HomePage")).Returns(false);
            mockAuthorizationService.Setup(m => m.IsAuthorized("EmployerFeature.CallToAction")).Returns(true);

            _controller = new EmployerTeamController(
                mockAuthenticationService.Object,
                mockMultiVariantTestingService.Object,
                mockCookieStorageService.Object,
                mockEmployerTeamOrchestrator.Object,
                mockAuthorizationService.Object);
        }

        [Test]
        public void ThenForNonLevyTheCheckFundingViewIsReturnedAtRow1Panel1()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                    AgreementsToSign = false
            };
            
            
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;
            
            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("CheckFunding", (result.Model as dynamic).ViewName);
            Assert.AreEqual(PanelType.Action, (result.Model as dynamic).PanelType);
        }

        [Test]
        public void ThenForNonLevyTheContinueSetupForSingleReservationViewIsReturnedAtRow1Panel1()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                AgreementsToSign = false,
                Reservations = new List<Reservation> { new Reservation { Status = ReservationStatus.Pending } },
                ApprenticeshipAdded = false
            };
            
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;                  

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ContinueSetupForSingleReservation", (result.Model as dynamic).ViewName);
            Assert.AreEqual(PanelType.Summary, (result.Model as dynamic).PanelType);
        }

        [Test]
        public void ThenForNonLevyTheYourSingleApprovedApprenticeViewIsReturnedAtRow1Panel1()
        {
            //Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;           
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                AgreementsToSign = false,
                Reservations = new List<Reservation> { new Reservation { Status = ReservationStatus.Completed } },                
                ApprenticeshipsCount = 1
            };            

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("YourSingleApprovedApprentice", (result.Model as dynamic).ViewName);
        }

        [Test]
        public void ThenForNonLevyTheContinueSetupForSingleApprenticeshipViewIsReturnedAtRow1Panel1()
        {
            //Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;            
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                AgreementsToSign = false,
                Reservations = new List<Reservation> { new Reservation { Status = ReservationStatus.Completed } },
                ApprenticeshipAdded = false,
                CohortsCount = 1,
                ApprenticeshipsCount = 0,
                NumberOfDraftApprentices = 1,                
                CohortStatus = Web.Extensions.CohortStatus.Draft                
            };

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ContinueSetupForSingleApprenticeship", (result.Model as dynamic).ViewName);
        }

        [Test]
        public void ThenForNonLevyTheYourSingleApprenticeWithTrainingProviderStatusViewIsReturnedAtRow1Panel1()
        {
            //Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;           
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                AgreementsToSign = false,
                Reservations = new List<Reservation> { new Reservation { Status = ReservationStatus.Completed } },
                ApprenticeshipAdded = false,
                CohortsCount = 1,
                ApprenticeshipsCount = 0,
                NumberOfDraftApprentices = 1,
                CohortStatus = Web.Extensions.CohortStatus.WithTrainingProvider
            };            

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("YourSingleApprenticeWithTrainingProviderStatus", (result.Model as dynamic).ViewName);
        }


        [Test]
        public void ThenForNonLevyTheYourSingleApprenticeReadyForReviewStatusViewIsReturnedAtRow1Panel1()
        {
            //Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.ApprenticeshipEmployerType = Common.Domain.Types.ApprenticeshipEmployerType.NonLevy;
            model.CallToActionViewModel = new CallToActionViewModel
            {
                AgreementsToSign = false,
                Reservations = new List<Reservation> { new Reservation { Status = ReservationStatus.Completed } },
                ApprenticeshipAdded = false,
                CohortsCount = 1,
                ApprenticeshipsCount = 0,
                NumberOfDraftApprentices = 1,
                CohortStatus = Web.Extensions.CohortStatus.Review
            };

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("YourSingleApprenticeReadyForReviewStatus", (result.Model as dynamic).ViewName);
        }
    }
}
