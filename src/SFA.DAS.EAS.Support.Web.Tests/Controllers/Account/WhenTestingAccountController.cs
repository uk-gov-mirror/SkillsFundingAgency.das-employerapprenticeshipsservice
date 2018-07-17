using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Web.Controllers;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Navigation;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;

namespace SFA.DAS.EAS.Support.Web.Tests.Controllers.Account
{
    public abstract class WhenTestingAccountController
    {
        protected Mock<IAccountHandler> AccountHandler;
        protected Mock<IPayeLevySubmissionsHandler> _payeLevySubmissionsHandler;
        protected Mock<ILog> _logger;
        protected Mock<IPayeLevyMapper> _payeLevyDeclarationMapper;
        protected AccountController Unit;

        protected Mock<IMenuService> _mockMenuService;
        protected Mock<IMenuTemplateTransformer> _mockMenuTemplateTransformer;
        protected Mock<IChallengeService> _mockIChallengeService;
        protected Mock<IChallengeRepository<PayeSchemeChallengeViewModel>> _mockPayeChallengeViewModelRepository;
        protected Mock<IIdentityHandler> _mockIdentityHandler;

        [SetUp]
        public void Setup()
        {
            AccountHandler = new Mock<IAccountHandler>();
            _payeLevySubmissionsHandler = new Mock<IPayeLevySubmissionsHandler>();
            _logger = new Mock<ILog>();
            _payeLevyDeclarationMapper = new Mock<IPayeLevyMapper>();
            _mockMenuService = new Mock<IMenuService>();
            _mockMenuTemplateTransformer = new Mock<IMenuTemplateTransformer>();
            _mockIChallengeService = new Mock<IChallengeService>();
            _mockPayeChallengeViewModelRepository = new Mock<IChallengeRepository<PayeSchemeChallengeViewModel>>();
            _mockIdentityHandler = new Mock<IIdentityHandler>();

            Unit = new AccountController(
                AccountHandler.Object,
                _payeLevySubmissionsHandler.Object,
                _logger.Object,
                _payeLevyDeclarationMapper.Object,
                _mockMenuService.Object,
                _mockMenuTemplateTransformer.Object,
                _mockIChallengeService.Object,
                _mockPayeChallengeViewModelRepository.Object,
                3,
                "https://localhost/",
                10,
                _mockIdentityHandler.Object);//
        }
    }
}