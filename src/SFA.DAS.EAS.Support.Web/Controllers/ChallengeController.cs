using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices;
using SFA.DAS.EAS.Support.ApplicationServices.Models;
using SFA.DAS.EAS.Support.Core.Models;
using SFA.DAS.EAS.Support.Infrastructure.Models;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.Support.Shared.Authentication;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    public class ChallengeController : Controller
    {
        private readonly IChallengeHandler _handler;

        public ChallengeController(IChallengeHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        [Route("accounts/{accountId}/challenges")]
        public async Task<ActionResult> Index(string accountId)
        {
            var response = await _handler.Get(accountId);

            if (response.StatusCode != SearchResponseCodes.Success)
                return HttpNotFound($"There was a problem finding the account {accountId}");

            return View(new ChallengeViewModel
            {
                Characters = response.Characters,
                Id = accountId
            });
        }

        [HttpPost]
        [Route("accounts/{accountId}/challenges")]
        public async Task<ActionResult> Index(string accountId, ChallengeEntry challengeEntry)
        {
            var response = await _handler.Handle(Map(challengeEntry));

            if (response.IsValid)
            {
                return Json(new ChallengeValidationResult
                {
                    IsValidResponse = true
                });
            }


            var model = new ChallengeViewModel
            {
                Characters = response.Characters,
                Id = challengeEntry.Id,
                Url = challengeEntry.Url,
                HasError = true
            };

            return View(model);
        }

        private ChallengePermissionQuery Map(ChallengeEntry challengeEntry)
        {
            return new ChallengePermissionQuery
            {
                Id = challengeEntry.Id,
                Url = challengeEntry.Url,
                ChallengeElement1 = challengeEntry.Challenge1,
                ChallengeElement2 = challengeEntry.Challenge2,
                Balance = challengeEntry.Balance,
                FirstCharacterPosition = challengeEntry.FirstCharacterPosition,
                SecondCharacterPosition = challengeEntry.SecondCharacterPosition
            };
        }
    }
}