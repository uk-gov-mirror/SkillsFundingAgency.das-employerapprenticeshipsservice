﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SFA.DAS.EmployerAccounts.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreement;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreementRemove;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreements;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreementsRemove;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementPdf;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementType;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationAgreements;
using SFA.DAS.EmployerAccounts.Queries.GetSignedEmployerAgreementPdf;
using SFA.DAS.EmployerAccounts.Queries.GetUnsignedEmployerAgreement;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Web.Orchestrators
{
    public class EmployerAgreementOrchestrator : UserVerificationOrchestratorBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IReferenceDataService _referenceDataService;

        protected EmployerAgreementOrchestrator()
        {
        }

        public EmployerAgreementOrchestrator(
            IMediator mediator,
            IMapper mapper,
            IReferenceDataService referenceDataService) : base(mediator)
        {
            _mediator = mediator;
            _mapper = mapper;
            _referenceDataService = referenceDataService;
        }

        public virtual async Task<OrchestratorResponse<EmployerAgreementListViewModel>> Get(string hashedId,
            string externalUserId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetAccountEmployerAgreementsRequest
                {
                    HashedAccountId = hashedId,
                    ExternalUserId = externalUserId
                });

                return new OrchestratorResponse<EmployerAgreementListViewModel>
                {
                    Data = new EmployerAgreementListViewModel
                    {
                        HashedAccountId = hashedId,
                        EmployerAgreementsData = response
                    }
                };
            }
            catch (Exception)
            {
                return new OrchestratorResponse<EmployerAgreementListViewModel>
                {
                    Status = HttpStatusCode.Unauthorized
                };
            }
        }


        public virtual async Task<OrchestratorResponse<EmployerAgreementViewModel>> GetById(
            string agreementid, string hashedId, string externalUserId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetEmployerAgreementRequest
                {
                    AgreementId = agreementid,
                    HashedAccountId = hashedId,
                    ExternalUserId = externalUserId
                });

                var employerAgreementView =
                    _mapper.Map<AgreementDto, EmployerAgreementView>(response.EmployerAgreement);

                var organisationLookupByIdPossible = await _referenceDataService.IsIdentifiableOrganisationType(employerAgreementView.LegalEntitySource);

                return new OrchestratorResponse<EmployerAgreementViewModel>
                {
                    Data = new EmployerAgreementViewModel
                    {
                        EmployerAgreement = employerAgreementView,
                        OrganisationLookupPossible = organisationLookupByIdPossible
                    }
                };
            }
            catch (InvalidRequestException ex)
            {
                return new OrchestratorResponse<EmployerAgreementViewModel>
                {
                    Status = HttpStatusCode.BadRequest,
                    Data = new EmployerAgreementViewModel(),
                    Exception = ex
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new OrchestratorResponse<EmployerAgreementViewModel>
                {
                    Status = HttpStatusCode.Unauthorized
                };
            }
        }

        public async Task<OrchestratorResponse<SignAgreementViewModel>> SignAgreement(string agreementid, string hashedId, string externalUserId, DateTime signedDate)
        {
            try
            {
                await _mediator.SendAsync(new SignEmployerAgreementCommand
                {
                    HashedAccountId = hashedId,
                    ExternalUserId = externalUserId,
                    SignedDate = signedDate,
                    HashedAgreementId = agreementid
                });

                var unsignedAgreement = await _mediator.SendAsync(new GetUnsignedEmployerAgreementRequest
                { 
                    ExternalUserId = externalUserId,
                    HashedAccountId = hashedId
                });

                var agreementType = await _mediator.SendAsync(new GetEmployerAgreementTypeRequest { HashedAgreementId = agreementid });

                return new OrchestratorResponse<SignAgreementViewModel>
                {
                    Data = new SignAgreementViewModel
                    {
                        HasFurtherPendingAgreements = !string.IsNullOrEmpty(unsignedAgreement.HashedAgreementId),
                        SignedAgreementType = agreementType.AgreementType
                    }
                };
            }
            catch (InvalidRequestException ex)
            {
                return new OrchestratorResponse<SignAgreementViewModel>
                {
                    Exception = ex,
                    Status = HttpStatusCode.BadRequest
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new OrchestratorResponse<SignAgreementViewModel>
                {
                    Status = HttpStatusCode.Unauthorized
                };
            }
        }
        public virtual async Task<OrchestratorResponse<bool>> RemoveLegalAgreement(ConfirmLegalAgreementToRemoveViewModel model, string userId)
        {
            var response = new OrchestratorResponse<bool>();
            try
            {
                if (model.RemoveOrganisation == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.FlashMessage =
                        FlashMessageViewModel.CreateErrorFlashMessageViewModel(new Dictionary<string, string>
                        {
                            {"RemoveOrganisation", "Confirm you wish to remove the organisation"}
                        });
                    return response;
                }

                if (model.RemoveOrganisation == 1)
                {
                    response.Status = HttpStatusCode.Continue;
                    return response;
                }

                await _mediator.SendAsync(new RemoveLegalEntityCommand
                {
                    HashedAccountId = model.HashedAccountId,
                    UserId = userId,
                    HashedLegalAgreementId = model.HashedAgreementId
                });

                response.FlashMessage = new FlashMessageViewModel
                {
                    Headline = $"You have removed {model.Name}.",
                    Severity = FlashMessageSeverityLevel.Success
                };
                response.Data = true;
            }
            catch (InvalidRequestException ex)
            {

                response.Status = HttpStatusCode.BadRequest;
                response.FlashMessage = FlashMessageViewModel.CreateErrorFlashMessageViewModel(ex.ErrorMessages);
                response.Exception = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                response.Status = HttpStatusCode.Unauthorized;
                response.Exception = ex;
            }

            return response;
        }
        public async Task<OrchestratorResponse<EmployerAgreementPdfViewModel>> GetPdfEmployerAgreement(string hashedAccountId, string agreementId, string userId)
        {
            var pdfEmployerAgreement = new OrchestratorResponse<EmployerAgreementPdfViewModel>();

            try
            {
                var result = await _mediator.SendAsync(new GetEmployerAgreementPdfRequest
                {
                    HashedAccountId = hashedAccountId,
                    HashedLegalAgreementId = agreementId,
                    UserId = userId
                });

                pdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel { PdfStream = result.FileStream };
            }
            catch (UnauthorizedAccessException)
            {
                pdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel();
                pdfEmployerAgreement.Status = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                pdfEmployerAgreement.Exception = ex;
                pdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel();
                pdfEmployerAgreement.Status = HttpStatusCode.NotFound;
            }

            return pdfEmployerAgreement;
        }

        public async Task<OrchestratorResponse<EmployerAgreementPdfViewModel>> GetSignedPdfEmployerAgreement(string hashedAccountId, string agreementId, string userId)
        {

            var signedPdfEmployerAgreement = new OrchestratorResponse<EmployerAgreementPdfViewModel>();

            try
            {
                var result =
                    await
                        _mediator.SendAsync(new GetSignedEmployerAgreementPdfRequest
                        {
                            HashedAccountId = hashedAccountId,
                            HashedLegalAgreementId = agreementId,
                            UserId = userId
                        });

                signedPdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel { PdfStream = result.FileStream };

                return signedPdfEmployerAgreement;
            }
            catch (InvalidRequestException ex)
            {
                signedPdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel();
                signedPdfEmployerAgreement.Status = HttpStatusCode.BadRequest;
                signedPdfEmployerAgreement.FlashMessage = new FlashMessageViewModel
                {
                    Headline = "Errors to fix",
                    Message = "Check the following details:",
                    ErrorMessages = ex.ErrorMessages,
                    Severity = FlashMessageSeverityLevel.Error
                };
            }
            catch (UnauthorizedAccessException)
            {
                signedPdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel();
                signedPdfEmployerAgreement.Status = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                signedPdfEmployerAgreement.Exception = ex;
                signedPdfEmployerAgreement.Data = new EmployerAgreementPdfViewModel();
                signedPdfEmployerAgreement.Status = HttpStatusCode.NotFound;
            }

            return signedPdfEmployerAgreement;

        }

        public virtual async Task<OrchestratorResponse<LegalAgreementsToRemoveViewModel>> GetLegalAgreementsToRemove(string hashedAccountId, string userId)
        {
            var response = new OrchestratorResponse<LegalAgreementsToRemoveViewModel>();
            try
            {
                var result = await _mediator.SendAsync(new GetAccountEmployerAgreementsRemoveRequest
                {
                    HashedAccountId = hashedAccountId,
                    UserId = userId
                });

                response.Data = new LegalAgreementsToRemoveViewModel
                {
                    Agreements = result.Agreements

                };
            }
            catch (InvalidRequestException ex)
            {
                response.Status = HttpStatusCode.BadRequest;
                response.FlashMessage = new FlashMessageViewModel
                {
                    Headline = "Errors to fix",
                    Message = "Check the following details:",
                    ErrorMessages = ex.ErrorMessages,
                    Severity = FlashMessageSeverityLevel.Error
                };
                response.Exception = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                response.Status = HttpStatusCode.Unauthorized;
                response.Exception = ex;
            }
            return response;
        }

        public virtual async Task<OrchestratorResponse<ConfirmLegalAgreementToRemoveViewModel>> GetConfirmRemoveOrganisationViewModel(string agreementId, string hashedAccountId, string userId)
        {
            var response = new OrchestratorResponse<ConfirmLegalAgreementToRemoveViewModel>();
            try
            {
                var result = await _mediator.SendAsync(new GetAccountEmployerAgreementRemoveRequest
                {
                    HashedAccountId = hashedAccountId,
                    UserId = userId,
                    HashedAgreementId = agreementId
                });
                response.Data = new ConfirmLegalAgreementToRemoveViewModel
                {
                    HashedAccountId = result.Agreement.HashedAccountId,
                    HashedAgreementId = result.Agreement.HashedAgreementId,
                    Id = result.Agreement.Id,
                    Name = result.Agreement.Name,
                    AgreementStatus = result.Agreement.Status
                };
            }
            catch (InvalidRequestException ex)
            {
                response.Status = HttpStatusCode.BadRequest;
                response.FlashMessage = new FlashMessageViewModel
                {
                    Headline = "Errors to fix",
                    Message = "Check the following details:",
                    ErrorMessages = ex.ErrorMessages,
                    Severity = FlashMessageSeverityLevel.Error
                };
                response.Exception = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                response.Status = HttpStatusCode.Unauthorized;
                response.Exception = ex;
            }

            return response;
        }

        public virtual async Task<OrchestratorResponse<OrganisationAgreementViewModelV1>> GetOrganisationAgreements(string accountLegalEntityHashedId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetOrganisationAgreementsRequest
                {
                    AccountLegalEntityHashedId = accountLegalEntityHashedId
                });

                var employerAgreementView =
                  _mapper.Map<OrganisationAgreement, OrganisationAgreementViewModel>(response.OrganisationAgreements);

                var organisationLookupByIdPossible = await _referenceDataService.IsIdentifiableOrganisationType(response.OrganisationAgreements.LegalEntity.Source);

                return new OrchestratorResponse<OrganisationAgreementViewModelV1>
                {
                    Data = new OrganisationAgreementViewModelV1
                    {
                        OrganisationAgreementViewModel = employerAgreementView,
                        OrganisationLookupPossible = organisationLookupByIdPossible
                    }
                };
            }
            catch (InvalidRequestException ex)
            {
                return new OrchestratorResponse<OrganisationAgreementViewModelV1>
                {
                    Status = HttpStatusCode.BadRequest,
                    Data = new OrganisationAgreementViewModelV1(),
                    Exception = ex
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new OrchestratorResponse<OrganisationAgreementViewModelV1>
                {
                    Status = HttpStatusCode.Unauthorized
                };
            }
        }
    }
}
   