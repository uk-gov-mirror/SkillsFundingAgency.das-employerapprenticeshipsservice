using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Discovery;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    [RoutePrefix("employers")]
    public class AccountController : Controller
    {
        private readonly IAccountHandler _accountHandler;
        private readonly IPayeLevySubmissionsHandler _payeLevySubmissionsHandler;
        private readonly ILog _log;
        private readonly IPayeLevyMapper _payeLevyMapper;

        public AccountController(IAccountHandler accountHandler,
            IPayeLevySubmissionsHandler payeLevySubmissionsHandler,
            ILog log,
            IPayeLevyMapper payeLevyDeclarationMapper)
        {
            _accountHandler = accountHandler;
            _payeLevySubmissionsHandler = payeLevySubmissionsHandler;
            _log = log;
            _payeLevyMapper = payeLevyDeclarationMapper;
        }

        [Route("accounts/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var response = await _accountHandler.FindOrganisations(id);

            if (response.StatusCode == SearchResponseCodes.Success)
            {
                var vm = new AccountDetailViewModel
                {
                    Account = response.Account,
                    AccountUri = $"views/resource/index/{{0}}?key={SupportServiceResourceKey.EmployerUser}"

                };

                return View(vm);
            }

            return HttpNotFound();
        }

        [Route("accounts/{accountId}/payeschemes")]
        public async Task<ActionResult> PayeSchemes(string accountId)
        {
            var response = await _accountHandler.FindPayeSchemes(accountId);

            if (response.StatusCode == SearchResponseCodes.Success)
            {
                var vm = new AccountDetailViewModel
                {
                    Account = response.Account,
                    AccountUri = $"views/employer/users/{{0}}"
                };

                return View(vm);
            }

            return new HttpNotFoundResult();
        }

        //[Route("accounts/header/{id}")]
        //public async Task<ActionResult> Header(string id)
        //{
        //    var response = await _accountHandler.Find(id);

        //    if (response.StatusCode != SearchResponseCodes.Success)
        //        return HttpNotFound();

        //    return View("SubHeader", response.Account);
        //}

        [Route("accounts/{accountId}/teams")]
        public async Task<ActionResult> Team(string accountId)
        {
            var response = await _accountHandler.FindTeamMembers(accountId);

            if (response.StatusCode == SearchResponseCodes.Success)
            {
                var vm = new AccountDetailViewModel
                {
                    Account = response.Account,
                    AccountUri = $"views/employers/users/{{0}}"
                };

                return View(vm);
            }

            return HttpNotFound();
        }

        [Route("accounts/{accountId}/finance")]
        public async Task<ActionResult> Finance(string accountId)
        {
            var response = await _accountHandler.FindFinance(accountId);

            if (response.StatusCode == SearchResponseCodes.Success)
            {
                var vm = new FinanceViewModel
                {
                    Account = response.Account,
                    Balance = response.Balance
                };

                return View(vm);
            }

            return HttpNotFound();
        }

        [Route("accounts/{accountId}/levysubmissions/{payeSchemeId}")]
        public async Task<ActionResult> PayeSchemeLevySubmissions(string accountId, string payeSchemeId)
        {
            var response = await _payeLevySubmissionsHandler.FindPayeSchemeLevySubmissions(accountId, payeSchemeId);

            if (response.StatusCode != PayeLevySubmissionsResponseCodes.AccountNotFound)
            {
                var model = _payeLevyMapper.MapPayeLevyDeclaration(response);

                model.UnexpectedError =
                    response.StatusCode == PayeLevySubmissionsResponseCodes.UnexpectedError;

                return View(model);
            }

            return HttpNotFound();
        }
    }
}