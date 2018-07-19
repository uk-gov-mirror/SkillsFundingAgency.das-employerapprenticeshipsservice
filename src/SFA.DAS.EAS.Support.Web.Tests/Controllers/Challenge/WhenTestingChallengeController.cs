using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Support.ApplicationServices;
using SFA.DAS.EAS.Support.Web.Controllers;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.Support.Shared.Challenge;


namespace SFA.DAS.EAS.Support.Web.Tests.Controllers.Challenge
{
    public abstract class WhenTestingChallengeController
    {
        protected Mock<IChallengeHandler> MockChallengeHandler;
        protected Mock<HttpContextBase> MockContextBase;
        protected Mock<HttpRequestBase> MockRequestBase;
        protected Mock<HttpResponseBase> MockResponseBase;
        protected Mock<IPrincipal> MockUser;
        protected RouteData RouteData;
        protected ChallengeController Unit;
        protected ControllerContext UnitControllerContext;


        protected Mock<IChallengeRepository<PayeSchemeChallengeViewModel>> MockChallengeRepository;
        


        [SetUp]
        public void Setup()
        {
            MockChallengeHandler = new Mock<IChallengeHandler>();
            MockChallengeRepository = new Mock<IChallengeRepository<PayeSchemeChallengeViewModel>>();

            Unit = new ChallengeController(MockChallengeRepository.Object, MockChallengeHandler.Object);

            RouteData = new RouteData();
            MockContextBase = new Mock<HttpContextBase>();

            MockRequestBase = new Mock<HttpRequestBase>();
            MockResponseBase = new Mock<HttpResponseBase>();
            MockUser = new Mock<IPrincipal>();

            MockContextBase.Setup(x => x.Request).Returns(MockRequestBase.Object);
            MockContextBase.Setup(x => x.Response).Returns(MockResponseBase.Object);
            MockContextBase.Setup(x => x.User).Returns(MockUser.Object);
            UnitControllerContext = new ControllerContext(MockContextBase.Object, RouteData, Unit);

            MockChallengeRepository.Setup(x => x.Retrieve(It.IsAny<Guid>()))
                .ReturnsAsync(new PayeSchemeChallengeViewModel(){});

            Unit.ControllerContext = UnitControllerContext;
        }
    }
}