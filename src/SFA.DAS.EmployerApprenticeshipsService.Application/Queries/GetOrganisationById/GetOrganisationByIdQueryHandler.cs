﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Application.Exceptions;
using SFA.DAS.EAS.Application.Queries.GetOrganisations;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Infrastructure.Extensions;

namespace SFA.DAS.EAS.Application.Queries.GetOrganisationById
{
    public class GetOrganisationByIdQueryHandler : IAsyncRequestHandler<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
    {
        private readonly IValidator<GetOrganisationByIdRequest> _validator;
        private readonly IReferenceDataService _referenceDataService;

        public GetOrganisationByIdQueryHandler(IValidator<GetOrganisationByIdRequest> validator, IReferenceDataService referenceDataService)
        {
            _validator = validator;
            _referenceDataService = referenceDataService;
        }

        public async Task<GetOrganisationByIdResponse> Handle(GetOrganisationByIdRequest message)
        {
            var valdiationResult = _validator.Validate(message);

            if (!valdiationResult.IsValid())
            {
                throw new InvalidRequestException(valdiationResult.ValidationDictionary);
            }

            var organisation =
                await _referenceDataService.GetLatestDetails(message.OrganisationType.ToReferenceDataOrganisationType(), message.Identifier);

            return new GetOrganisationByIdResponse { Organisation = organisation };
        }
    }
}
