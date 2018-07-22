using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EAS.Support.ApplicationServices;
using SFA.DAS.EAS.Support.Infrastructure.Models;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.Navigation;

namespace SFA.DAS.EAS.Support.Web.Controllers
{
    public class ChallengesController : TestBaseController
    {
        private readonly IChallengeRepository<PayeSchemeChallengeViewModel> _challengeRepository;
        private readonly IChallengeHandler _handler;
        public ChallengesController(
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeRepository,
            IChallengeHandler handler,
            IMenuService menuService,
            IMenuTemplateTransformer menuTemplateTransformer,
            IChallengeService challengeService,
            IChallengeRepository<PayeSchemeChallengeViewModel> challengeHandler,
            IIdentityHandler identityHandler) :
            base(menuService, menuTemplateTransformer, challengeService, identityHandler)
        {
            _challengeRepository = challengeRepository;
            _handler = handler;
        }

        [HttpGet]
        [Route("challenges/{challengeId:guid}")]
        public async Task<ActionResult> Challenge(Guid challengeId)
        {
            var model = await _challengeRepository.Retrieve(challengeId);
            if (model == null)
            {
                return View("_notFound", new { Identifiers = new Dictionary<string, string>() { { "Challenge Id", $"{challengeId}" } } });
            }

            await RestoreChallengeSummary(model);
            return View("Index", model);
        }

        [HttpPost]
        [Route("challenges/response")]
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

            await RestoreChallengeSummary(challenge);
            model.Characters = response.Characters;
            model.HasError = true;
            return View("Index", model);
        }

        private async Task RestoreChallengeSummary(PayeSchemeChallengeViewModel challenge)
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
    }
}