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
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.Navigation;
using SFA.DAS.Support.Shared.ViewModels;
using SupportAgentChallenge = SFA.DAS.Support.Shared.Challenge.SupportAgentChallenge;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
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

                RedirectToAction("Challenge", "Challenges", new {challengeId});
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

                RedirectToAction("Challenge", "Challenges", new {challengeId});
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

                RedirectToAction("Challenge", "Challenges", new {challengeId});
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