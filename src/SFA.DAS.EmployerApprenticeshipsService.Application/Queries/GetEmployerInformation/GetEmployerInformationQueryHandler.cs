﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerApprenticeshipsService.Application.Interfaces;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetEmployerInformation
{
    public class GetEmployerInformationQueryHandler : IAsyncRequestHandler<GetEmployerInformationRequest, GetEmployerInformationResponse>
    {
        private readonly IEmployerVerificationService _employerVerificationService;

        public GetEmployerInformationQueryHandler(IEmployerVerificationService employerVerificationService)
        {
            if (employerVerificationService == null)
                throw new ArgumentNullException(nameof(employerVerificationService));
            _employerVerificationService = employerVerificationService;
        }

        public async Task<GetEmployerInformationResponse> Handle(GetEmployerInformationRequest message)
        {
            var employer = await _employerVerificationService.GetInformation(message.Id);

            if (employer == null)
                return null;

            return new GetEmployerInformationResponse
            {
                CompanyNumber = employer.CompanyNumber,
                CompanyName = employer.CompanyName,
                DateOfIncorporation = employer.DateOfIncorporation
            };
        }
    }
}