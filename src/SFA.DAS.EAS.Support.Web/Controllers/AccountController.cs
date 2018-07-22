using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Infrastructure.Models;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.Discovery;
using SFA.DAS.Support.Shared.Navigation;
using SFA.DAS.Support.Shared.ViewModels;
using SupportAgentChallenge = SFA.DAS.Support.Shared.Challenge.SupportAgentChallenge;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    [RoutePrefix("employers")]
    public class AccountController : TestBaseController
    {
        private readonly IChallengeRepository<PayeSchemeChallengeViewModel> _challengeRepository;
        private readonly IChallengeHandler _handler;
        private const string ChallengeEntityType = "account";
        private readonly IAccountHandler _accountHandler;
        private readonly IChallengeRepository<PayeSchemeChallengeViewModel> _challengeViewModeleRepository;
        private readonly ILog _log;
        private readonly IPayeLevyMapper _payeLevyMapper;
        private readonly IPayeLevySubmissionsHandler _payeLevySubmissionsHandler;
        private readonly string _baseUriString;

        public AccountController(
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeRepository,
            IChallengeHandler handler,
            IAccountHandler accountHandler,
            IPayeLevySubmissionsHandler payeLevySubmissionsHandler,
            ILog log,
            IPayeLevyMapper payeLevyDeclarationMapper,
            IMenuService menuService,
            IMenuTemplateTransformer menuTemplateTransformer,
            IChallengeService challengeService,
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeHandler,
            IIdentityHandler identityHandler) :
            base(menuService, menuTemplateTransformer, challengeService, identityHandler)
        {
            _challengeRepository = challengeRepository;
            _handler = handler;
            _accountHandler = accountHandler;
            _payeLevySubmissionsHandler = payeLevySubmissionsHandler;
            _log = log;
            _payeLevyMapper = payeLevyDeclarationMapper;
            _challengeViewModeleRepository = challengeHandler;

            MenuPerspective = SupportMenuPerspectives.EmployerAccount; // all methods on this controller are related to this menu
            _baseUriString = $"{Request?.Url?.Scheme ?? "https"}://{Request?.Url?.Authority ?? "localhost:44349"}/{Request?.ApplicationPath ?? $"{SupportServiceIdentity.SupportEmployerAccount.ToRoutePrefix()}".TrimEnd('/')}/";

        }

        [Route("accounts/{accountId}")]
        public async Task<ActionResult> Index(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });

            var response = await _accountHandler.FindOrganisations(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });

            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employerusers/users/{{0}}"
            };


            MenuSelection = "Account.Organisations";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> { { "accountId", $"{response.Account.AccountId}" } };

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/paye")]
        public async Task<ActionResult> PayeSchemes(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(PayeSchemes)}");
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> { { "accountId", $"{accountId}" } };
            MenuSelection = "Account.Finance.PAYE";

            var challengeId = await ChallengeService.IsNeeded(RequestIdentity, ChallengeEntityType, accountId);

            if (challengeId != Guid.Empty)
            {
                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                return await Challenge(challengeId);

            }

            _log.Trace(
                $"Challenge is satisfied for {RequestIdentity} on Account {accountId}, obtaining requested information...");

            var response = await _accountHandler.FindPayeSchemes(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
            {
                _log.Trace($"PAYE schemes for Account {accountId} are not avaialble.");

                return View("_notFound",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });
            }


            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employer/users/{{0}}"
            };
            ViewBag.Header = BuildHeader(response.Account);


            return View(vm);
        }

        [Route("accounts/{accountId}/teams")]
        public async Task<ActionResult> Team(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });

            var response = await _accountHandler.FindTeamMembers(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });

            var vm = new AccountDetailViewModel
            {
                Account = response.Account,
                AccountUri = $"views/employerusers/users/{{0}}"
            };

            MenuSelection = "Account.Teams";
            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> { { "accountId", $"{response.Account.AccountId}" } };

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/transactions")]
        public async Task<ActionResult> Finance(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(Finance)}");
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });
            }

            MenuTransformationIdentifiers = new Dictionary<string, string> { { "accountId", $"{accountId}" } };
            MenuSelection = "Account.Finance.Transactions";

            var challengeId = await ChallengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {

                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                return await Challenge(challengeId);

            }


            var response = await _accountHandler.FindFinance(accountId);


            if (response.StatusCode != SearchResponseCodes.Success)
                return View("_notFound",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });
            var vm = new FinanceViewModel
            {
                Account = response.Account,
                Balance = response.Balance
            };

            ViewBag.Header = BuildHeader(response.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> { { "accountId", $"{response.Account.AccountId}" } };

            return View(vm);
        }

        [Route("accounts/{accountId}/finance/paye/{payeSchemeId}")]
        public async Task<ActionResult> PayeSchemeLevySubmissions(string accountId, string payeSchemeId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                _log.Warn($"Unspecified account passed to {nameof(PayeSchemeLevySubmissions)}");
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });
            }

            if (string.IsNullOrWhiteSpace(accountId))
                return View("_notUnderstood",
                    new { Identifiers = new Dictionary<string, string> { { "Account Id", $"{accountId}" } } });

            MenuTransformationIdentifiers = new Dictionary<string, string> { { "accountId", $"{accountId}" } };
            MenuSelection = "Account.Finance.PAYE.Submissions";

            var challengeId = await ChallengeService.IsNeeded(
                RequestIdentity,
                ChallengeEntityType,
                accountId
            );

            if (challengeId != Guid.Empty)
            {

                _log.Trace($"Challenge is requried for {RequestIdentity} on Account {accountId}");

                var supportAgentChallenge = await SaveChallengeSummary(accountId, challengeId, ChallengeEntityType);

                await SaveChallengeDetail(accountId, supportAgentChallenge);

                return await Challenge(challengeId);
            }


            var response = await _payeLevySubmissionsHandler.FindPayeSchemeLevySubmissions(accountId, payeSchemeId);

            var model = _payeLevyMapper.MapPayeLevyDeclaration(response);

            model.UnexpectedError = response.StatusCode == PayeLevySubmissionsResponseCodes.UnexpectedError;

            var accountResponse = await _accountHandler.FindOrganisations(accountId);

            ViewBag.Header = BuildHeader(accountResponse.Account);
            MenuTransformationIdentifiers =
                new Dictionary<string, string> { { "accountId", $"{accountId}" }, { "payeSchemeId", payeSchemeId } };

            return View(model);
        }


        private async Task<ActionResult> Challenge(Guid challengeId)
        {
            var model = await _challengeRepository.Retrieve(challengeId);
            if (model == null)
            {
                return View("_notFound", new { Identifiers = new Dictionary<string, string>() { { "Challenge Id", $"{challengeId}" } } });
            }

            RestoreChallengeSummary(model);
            return View("Challenge", model);
        }

        [HttpPost]
        [Route("challenge/response")]
        public async Task<ActionResult> Response(PayeSchemeChallengeViewModel model)
        {

            var challenge = await _challengeRepository.Retrieve(model.ChallengeId);

            if (challenge == null)
            {
                return View("_notFound", new { Identifiers = new Dictionary<string, string>() { { "Challenge Id", $"{model.ChallengeId}" } } });
            }

            var response = await _handler.Handle(Map(model));

            if (response.IsValid)
            {
                return Redirect(model.ReturnTo);
            }

            RestoreChallengeSummary(challenge);
            model.Characters = response.Characters;
            model.HasError = true;
            return View("Challenge", model);
        }

        private void RestoreChallengeSummary(PayeSchemeChallengeViewModel challenge)
        {

            RequestIdentity = challenge.Identity;
            MenuPerspective = challenge.MenuType;
            MenuSelection = challenge.MenuSelection;
            MenuTransformationIdentifiers = challenge.Identifiers;
        }

        private ChallengePermissionQuery Map(PayeSchemeChallengeViewModel model)
        {
            return new ChallengePermissionQuery
            {
                Id = model.ChallengeId.ToString(),
                Url = model.ReturnTo,
                ChallengeElement1 = model.Challenge1,
                ChallengeElement2 = model.Challenge2,
                Balance = model.Balance,
                FirstCharacterPosition = model.FirstCharacterPosition,
                SecondCharacterPosition = model.SecondCharacterPosition
            };
        }

        private async Task SaveChallengeDetail(string accountId, SupportAgentChallenge challenge)
        {
            var challengeViewModel = new PayeSchemeChallengeViewModel
            {
                ChallengeId = challenge.Id,
                Balance = "0",
                Characters = new List<int>(),
                EntityType = ChallengeEntityType,
                Identity = RequestIdentity,
                ResponseUrl = new Uri(new Uri(_baseUriString), $"{SupportServiceIdentity.SupportEmployerAccount.ToRoutePrefix()}/challenge/response").OriginalString,
                Identifier = accountId,
                MaxTries = ChallengeService.ChallengeMaxRetries,
                Tries = 1,
                Challenge = "",
                Message = "",
                MenuType = MenuPerspective,
                ReturnTo = Request.RawUrl,
                Identifiers = MenuTransformationIdentifiers,
                MenuSelection = MenuSelection
            };

            await _challengeViewModeleRepository.Store(challengeViewModel);
        }

        private static HeaderViewModel BuildHeader(Core.Models.Account account)
        {
            var prefix = $"<div>{account.DasAccountName}</div>";

            var accountIdInfo = $@"<div><span class=""heading-secondary""> Account ID {account.PublicHashedAccountId}, created {account.DateRegistered:dd/MM/yyyy}</span></div>";
            return new HeaderViewModel
            {
                Content = new HtmlString($"{prefix}{accountIdInfo}")
            };
        }
    }
}