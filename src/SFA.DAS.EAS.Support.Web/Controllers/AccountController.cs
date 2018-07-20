using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.Navigation;
using SFA.DAS.Support.Shared.ViewModels;
using SupportAgentChallenge = SFA.DAS.Support.Shared.Challenge.SupportAgentChallenge;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    public abstract class TestBaseController : Controller
    {
        public const string ResourceIdentityHeader = "X-ResourceIdentity";
        public const string ResourceRequestHeader = "X-ResourceRequest";

        protected static readonly MenuRoot EmptyMenu =
            new MenuRoot { MenuItems = new List<MenuItem>(), Perspective = SupportMenuPerspectives.None };

        private readonly IIdentityHandler _identityHandler;

        private readonly MenuViewModel _menuViewModel = new MenuViewModel { MenuOrientation = MenuOrientations.Vertical };

        protected readonly IChallengeService ChallengeService;
        protected readonly IMenuService MenuService;
        protected readonly IMenuTemplateTransformer MenuTemplateTransformer;


        protected BaseController(IMenuService menuService,
            IMenuTemplateTransformer menuTemplateTransformer,
            IChallengeService challengeService,
            IIdentityHandler identityHandler)
        {
            MenuService = menuService;
            MenuTemplateTransformer = menuTemplateTransformer;
            ChallengeService = challengeService;
            _identityHandler = identityHandler;
        }

        protected MenuRoot RootMenu { get; set; } = EmptyMenu;

        protected Dictionary<string, string> MenuTransformationIdentifiers { get; set; } =
            new Dictionary<string, string>();

        protected SupportMenuPerspectives MenuPerspective { get; set; } = SupportMenuPerspectives.None;
        protected string MenuSelection { get; set; } = null;

        public string RequestIdentity { get; set; }

        /// <summary>
        ///     Signals to the 'OnActionExecuted' to ignore the menu generation processing if this request a resource (sub-view)
        /// </summary>
        public bool IsAResourceRequest => HttpContext.Request.Headers.AllKeys.Contains(ResourceRequestHeader);

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            RequestIdentity = _identityHandler.GetIdentity(Request);

            base.OnActionExecuted(filterContext);

            if (MenuSelection == null || MenuPerspective == SupportMenuPerspectives.None || IsAResourceRequest) return;

            ProcessMenu();
        }


        /// <summary>
        ///     Sets up the view bag to drive the CSHMTL menu rendering within the Views/Shared/PanelLayout.cshtml and
        ///     Views/Shared/_navigation.cshtml views
        /// </summary>
        private void ProcessMenu()
        {
            RootMenu = MenuService.GetMenu(MenuPerspective).Result.FirstOrDefault();

            if (RootMenu == null) return;

            var menuItems =
                MenuTemplateTransformer.TransformMenuTemplates(RootMenu.MenuItems, MenuTransformationIdentifiers);

            if (!menuItems.Any()) return;

            _menuViewModel.SetMenu(menuItems, MenuSelection);

            ViewBag.Menu = _menuViewModel;
        }


        protected async Task<SupportAgentChallenge> SaveChallengeSummary(string accountId, Guid challengeId,
            string entityType)
        {
            var challenge = new SupportAgentChallenge
            {
                Id = challengeId,
                Identity = RequestIdentity,
                EntityType = entityType,
                EntityKey = accountId,
                Expires = DateTimeOffset.UtcNow.AddMinutes(ChallengeService.ChallengeExpiryMinutes)
            };

            await ChallengeService.Store(challenge);
            return challenge;
        }
    }
    [RoutePrefix("employers")]
    public class AccountController : TestBaseController
    {
        private const string ChallengeEntityType = "account";
        private readonly IAccountHandler _accountHandler;
        private readonly IChallengeRepository<PayeSchemeChallengeViewModel> _challengeHandler;
        private readonly ILog _log;
        private readonly IPayeLevyMapper _payeLevyMapper;
        private readonly IPayeLevySubmissionsHandler _payeLevySubmissionsHandler;

        public AccountController(
            IAccountHandler accountHandler,
            IPayeLevySubmissionsHandler payeLevySubmissionsHandler,
            ILog log,
            IPayeLevyMapper payeLevyDeclarationMapper,
            IMenuService menuService,
            IMenuTemplateTransformer menuTemplateTransformer,
            IChallengeService challengeService,
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeHandler,
            IIdentityHandler identityHandler) : 
            base(menuService,menuTemplateTransformer, challengeService,identityHandler)
        {
            _accountHandler = accountHandler;
            _payeLevySubmissionsHandler = payeLevySubmissionsHandler;
            _log = log;
            _payeLevyMapper = payeLevyDeclarationMapper;
            _challengeHandler = challengeHandler;

            MenuPerspective =
                SupportMenuPerspectives.EmployerAccount; // all methods on this controller are related to this menu
        }

        [Route("accounts/{accountId}")]
        public async Task<ActionResult> Index(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});

            var response = await _accountHandler.FindOrganisations(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});

            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employerusers/users/{{0}}"
            };


            MenuSelection = "Account.Organisations";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{response.Account.AccountId}"}};

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/paye")]
        public async Task<ActionResult> PayeSchemes(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(PayeSchemes)}");
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> {{"accountId", $"{accountId}"}};

            var challengeId = await ChallengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {
                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, AccountController.ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                RedirectToAction("Challenge", "Challenge", new {challengeId});
            }

            _log.Trace(
                $"Challenge is satisfied for {RequestIdentity} on Account {accountId}, obtaining requested information...");

            var response = await _accountHandler.FindPayeSchemes(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
            {
                _log.Trace($"PAYE schemes for Account {accountId} are not avaialble.");

                return View("_notFound",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }


            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employer/users/{{0}}"
            };

            MenuSelection = "Account.Finance.PAYE";
            ViewBag.Header = BuildHeader(response.Account);

            return View(vm);
        }
       
        [Route("accounts/{accountId}/teams")]
        public async Task<ActionResult> Team(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});

            var response = await _accountHandler.FindTeamMembers(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});

            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employerusers/users/{{0}}"
            };

            MenuSelection = "Account.Teams";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{response.Account.AccountId}"}};

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/transactions")]
        public async Task<ActionResult> Finance(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(Finance)}");
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> {{"accountId", $"{accountId}"}};

            var challengeId = await ChallengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {

                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, AccountController.ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                RedirectToAction("Challenge", "Challenge", new {challengeId});
            }


            var response = await _accountHandler.FindFinance(accountId);


            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            var vm = new FinanceViewModel
            {
                Account = response.Account,
                Balance = response.Balance
            };

            MenuSelection = "Account.Finance.Transactions";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{response.Account.AccountId}"}};

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/paye/{payeSchemeId}")]
        public async Task<ActionResult> PayeSchemeLevySubmissions(string accountId, string payeSchemeId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(PayeSchemeLevySubmissions)}");
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }

            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});

            MenuTransformationIdentifiers = new Dictionary<string, string> {{"accountId", $"{accountId}"}};

            var challengeId = await ChallengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {

                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, AccountController.ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                RedirectToAction("Challenge", "Challenge", new {challengeId});
            }


            var response = await _payeLevySubmissionsHandler.FindPayeSchemeLevySubmissions(accountId, payeSchemeId);

            var model = _payeLevyMapper.MapPayeLevyDeclaration(response);

            model.UnexpectedError = response.StatusCode == PayeLevySubmissionsResponseCodes.UnexpectedError;

            var accountResponse = await _accountHandler.FindOrganisations(accountId);

            MenuSelection = "Account.Finance.PAYE.Submissions";
            ViewBag.Header = BuildHeader(accountResponse.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{accountId}"}, {"payeSchemeId", payeSchemeId}};

            return View(model);
        }
    
        protected async Task SaveChallengeDetail(string accountId, SupportAgentChallenge challenge)
        {



            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}/{Request.ApplicationPath.TrimEnd('/')}/";

            var challengeViewModel = new PayeSchemeChallengeViewModel
            {
                ChallengeId = challenge.Id,
                Balance = "0",
                Characters = new List<int>(),
                EntityType = ChallengeEntityType,
                Identity = RequestIdentity,
                ResponseUrl = new Uri(new Uri(baseUrl), "/employers/challenges/response").OriginalString,
                Identifier = accountId,
                MaxTries = ChallengeService.ChallengeMaxRetries,
                Tries = 1,
                Challenge = "",
                Message = "",
                MenuType = MenuPerspective,
                ReturnTo = Request.RawUrl,
                Identifiers = MenuTransformationIdentifiers
            };

            await _challengeHandler.Store(challengeViewModel);
        }

        private static HeaderViewModel BuildHeader(Core.Models.Account account)
        {
            var prefix = $"<div>{account.DasAccountName}</div>";

            var accountIdInfo  = $@"<div><span class=""heading-secondary""> Account ID {account.PublicHashedAccountId}, created {account.DateRegistered:dd/MM/yyyy}</span></div>";
            return new HeaderViewModel
            {
                Content = new HtmlString($"{prefix}{accountIdInfo}" )
            };
        }
}
}