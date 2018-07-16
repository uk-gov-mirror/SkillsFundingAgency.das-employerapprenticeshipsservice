using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Navigation;
using SFA.DAS.Support.Shared.ViewModels;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    [RoutePrefix("employers")]
    public class AccountController : BaseController
    {
        private const string ChallengeEntityType = "account";
        private readonly IAccountHandler _accountHandler;

        /// Pull up to basecontroller
        private readonly int _challengeExpiryMinutes;
        /// Pull up to basecontroller
        private readonly IChallengeRepository<PayeSchemeChallengeViewModel> _challengeHandler;
        /// Pull up to basecontroller
        private readonly int _challengeMaxTries;
        /// Pull up to basecontroller
        private readonly IChallengeService _challengeService;
        private readonly ILog _log;
        private readonly IPayeLevyMapper _payeLevyMapper;
        private readonly IPayeLevySubmissionsHandler _payeLevySubmissionsHandler;
        /// Pull up to basecontroller
        private readonly Uri _thisUri;
        /// Pull up to basecontroller
        private readonly string RequestIdentity = "anonymous"; // Pull up to BaseController and hook in cross site Identity transfer.

        public AccountController(
            IAccountHandler accountHandler,
            IPayeLevySubmissionsHandler payeLevySubmissionsHandler,
            ILog log,
            IPayeLevyMapper payeLevyDeclarationMapper,
            IMenuService menuService,
            IMenuTemplateTransformer menuTemplateTransformer,
            IChallengeService challengeService,
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeHandler,
            int challengeMaxTries, string thisUri, int challengeExpiryMinutes) : base(menuService,
            menuTemplateTransformer)
        {
            _accountHandler = accountHandler;
            _payeLevySubmissionsHandler = payeLevySubmissionsHandler;
            _log = log;
            _payeLevyMapper = payeLevyDeclarationMapper;
            _challengeService = challengeService;
            _challengeHandler = challengeHandler;
            _challengeMaxTries = challengeMaxTries;
            _challengeExpiryMinutes = challengeExpiryMinutes;
            _thisUri = new Uri(thisUri);

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
                AccountUri = $"views/employers/users/{{0}}"
            };


            MenuSelection = "Account.Organisation";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{response.Account.AccountId}"}};

            return View(vm);
        }

        [Route("accounts/{accountId}/payeschemes")]
        public async Task<ActionResult> PayeSchemes(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(PayeSchemes)}");
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> {{"accountId", $"{accountId}"}};

            var challengeId = await _challengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {
                
                await SaveChallengeDetail(accountId, 
                    await SaveChallengeSummary(accountId, challengeId));

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
        /// <summary>
        /// Pull up to basecontroller
        /// </summary>
        protected async Task SaveChallengeDetail(string accountId, SupportAgentChallenge challenge)
        {
            var challengeViewModel = new PayeSchemeChallengeViewModel
            {
                ChallengeId = challenge.Id,
                Balance = "0",
                Characters = new List<int>(),
                EntityType = ChallengeEntityType,
                Identity = RequestIdentity,
                ResponseUrl = new Uri(_thisUri, "/employers/challenges/response").OriginalString,
                Identifier = accountId,
                MaxTries = _challengeMaxTries,
                Tries = 1,
                Challenge = "",
                Message = "",
                MenuType = MenuPerspective,
                ReturnTo = Request.RawUrl,
                Identifiers = MenuTransformationIdentifiers
            };

            await _challengeHandler.Store(challengeViewModel);
        }

        /// <summary>
        /// Pull up to basecontroller
        /// </summary>
        protected async Task<SupportAgentChallenge> SaveChallengeSummary(string accountId, Guid challengeId)
        {
            _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

            var challenge = new SupportAgentChallenge
            {
                Id = challengeId,
                Identity = RequestIdentity,
                EntityType = ChallengeEntityType,
                EntityKey = accountId,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_challengeExpiryMinutes)
            };

            await _challengeService.Store(challenge);
            return challenge;
        }

        private static HeaderViewModel BuildHeader(Core.Models.Account account)
        {
            return new HeaderViewModel
            {
                Content = new HtmlString(
                    $@"<span class=""heading - secondary""> Account ID @({account.PublicHashedAccountId}), created @{
                            account.DateRegistered
                        :dd / MM / yyyy}</span>")
            };
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
                AccountUri = $"views/employers/users/{{0}}"
            };

            MenuSelection = "Account.Teams";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> {{"accountId", $"{response.Account.AccountId}"}};

            return View(vm);
        }

        [Route("accounts/{accountId}/transactions")]
        public async Task<ActionResult> Finance(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(Finance)}");
                return View("_notUnderstood",
                    new {Identifiers = new Dictionary<string, string> {{"Account Id", $"{accountId}"}}});
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> {{"accountId", $"{accountId}"}};

            var challengeId = await _challengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {
                await SaveChallengeDetail(accountId,
                    await SaveChallengeSummary(accountId, challengeId));

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

        [Route("accounts/{accountId}/levysubmissions/{payeSchemeId}")]
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

            var challengeId = await _challengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {
                await SaveChallengeDetail(accountId,
                    await SaveChallengeSummary(accountId, challengeId));

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
    }
}