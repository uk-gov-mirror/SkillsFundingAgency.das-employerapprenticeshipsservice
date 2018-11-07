﻿using AutoMapper;
using MediatR;
using SFA.DAS.EAS.Application.Queries.GetEmployerAgreement;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.Organisation;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EAS.Web.Extensions;

namespace SFA.DAS.EAS.Web.Controllers
{
    [Authorize]
    [RoutePrefix("accounts/{HashedAccountId}")]
    public class EmployerAgreementController : BaseController
    {
        private readonly EmployerAgreementOrchestrator _orchestrator;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public EmployerAgreementController(IAuthenticationService owinWrapper,
            EmployerAgreementOrchestrator orchestrator,
            IAuthorizationService authorization,
            IMultiVariantTestingService multiVariantTestingService,
            ICookieStorageService<FlashMessageViewModel> flashMessage,
            IMediator mediator,
            IMapper mapper)
            : base(owinWrapper, multiVariantTestingService, flashMessage)
        {
            if (owinWrapper == null)
                throw new ArgumentNullException(nameof(owinWrapper));
            if (orchestrator == null)
                throw new ArgumentNullException(nameof(orchestrator));

            _orchestrator = orchestrator;
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("agreements")]
        public async Task<ActionResult> Index(string hashedAccountId, bool agreementSigned = false)
        {
            return Redirect(Url.EmployerAccountsAction("agreements"));
        }

        [HttpGet]
        [Route("agreements/{agreementId}/details")]
        public async Task<ActionResult> Details(string agreementId, string hashedAccountId, FlashMessageViewModel flashMessage)
        {
           
            return
                Redirect(Url.EmployerAccountsAction($"agreements/{agreementId}/details"));
        }

        [HttpGet]
        [Route("agreements/{agreementId}/view")]
        public async Task<ActionResult> View(string agreementId, string hashedAccountId, FlashMessageViewModel flashMessage)
        {
           return Redirect(Url.EmployerAccountsAction($"agreements/{agreementId}/view"));
        }

        [HttpGet]
        [Route("agreements/unsigned/view")]
        public async Task<ActionResult> ViewUnsignedAgreements(string hashedAccountId)
        {
            return Redirect(Url.EmployerAccountsAction("agreements/unsigned/view"));
        }

        [HttpGet]
        [Route("agreements/{agreementId}/about-your-agreement")]
        public async Task<ActionResult> AboutYourAgreement(string agreementid, string hashedAccountId)
        {
            return Redirect(Url.EmployerAccountsAction($"agreements/{agreementid}/about-your-agreement"));

        }

        [HttpGet]
        [Route("agreements/{agreementId}/sign-your-agreement")]
        public async Task<ActionResult> SignAgreement(GetEmployerAgreementRequest request)
        {
            request.ExternalUserId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            var response = await _mediator.SendAsync(request);
            var viewModel = _mapper.Map<GetEmployerAgreementResponse, EmployerAgreementViewModel>(response);

            return View(viewModel);
        }

        [HttpPost]
        [Route("agreements/{agreementId}/sign")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Sign(string agreementId, string hashedAccountId)
        {
            return Redirect(Url.EmployerAccountsAction($"agreements/{agreementId}/sign"));
            
        }

        [HttpGet]
        [Route("agreements/{agreementId}/next")]
        public async Task<ActionResult> NextSteps(string hashedAccountId)
        {
            var userId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            var userShownWizard = await _orchestrator.UserShownWizard(userId, hashedAccountId);

            var model = new OrchestratorResponse<EmployerAgreementNextStepsViewModel>
            {
                FlashMessage = GetFlashMessageViewModelFromCookie(),
                Data = new EmployerAgreementNextStepsViewModel { UserShownWizard = userShownWizard }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("agreements/{agreementId}/next")]
        public async Task<ActionResult> NextSteps(int? choice, string hashedAccountId)
        {
            var userId = OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName);

            var userShownWizard = await _orchestrator.UserShownWizard(userId, hashedAccountId);

            switch (choice ?? 0)
            {
                case 1: return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.EmployerAgreementControllerName);
                case 2: return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.TransfersControllerName);
                case 3: return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.EmployerTeamControllerName);
                default:
                    var model = new OrchestratorResponse<EmployerAgreementNextStepsViewModel>
                    {
                        FlashMessage = GetFlashMessageViewModelFromCookie(),
                        Data = new EmployerAgreementNextStepsViewModel
                        {
                            ErrorMessage = "You must select an option to continue.",
                            UserShownWizard = userShownWizard
                        }
                    };
                    return View(model); //No option entered
            }
        }

        [HttpGet]
        [Route("agreements/{agreementId}/agreement-pdf")]
        public async Task<ActionResult> GetPdfAgreement(string agreementId, string hashedAccountId)
        {

            var stream = await _orchestrator.GetPdfEmployerAgreement(hashedAccountId, agreementId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            if (stream.Data.PdfStream == null)
            {
                // ReSharper disable once Mvc.ViewNotResolved
                return View(stream);
            }

            return new FileStreamResult(stream.Data.PdfStream, ControllerConstants.PdfContentTypeName);
        }

        [HttpGet]
        [Route("agreements/{agreementId}/signed-agreement-pdf")]
        public async Task<ActionResult> GetSignedPdfAgreement(string agreementId, string hashedAccountId)
        {

            var stream = await _orchestrator.GetSignedPdfEmployerAgreement(hashedAccountId, agreementId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            if (stream.Data.PdfStream == null)
            {
                // ReSharper disable once Mvc.ViewNotResolved
                return View(stream);
            }

            return new FileStreamResult(stream.Data.PdfStream, ControllerConstants.PdfContentTypeName);
        }

        [HttpGet]
        [Route("agreements/remove")]
        public async Task<ActionResult> GetOrganisationsToRemove(string hashedAccountId)
        {
            var model = await _orchestrator.GetLegalAgreementsToRemove(hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            return View(model);
        }

        [HttpGet]
        [Route("agreements/remove/{agreementId}")]
        public async Task<ActionResult> ConfirmRemoveOrganisation(string agreementId, string hashedAccountId)
        {
            var model = await _orchestrator.GetConfirmRemoveOrganisationViewModel(agreementId, hashedAccountId, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

            var flashMessage = GetFlashMessageViewModelFromCookie();
            if (flashMessage != null)
            {
                model.FlashMessage = flashMessage;
                model.Data.ErrorDictionary = model.FlashMessage.ErrorMessages;
            }

            return View(model);
        }

        [HttpPost]
        [Route("agreements/remove/{agreementId}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveOrganisation(string hashedAccountId, string agreementId, ConfirmLegalAgreementToRemoveViewModel model)
        {
            return Redirect(Url.EmployerAccountsAction($"agreements/remove/{agreementId}"));
        }

    }
}