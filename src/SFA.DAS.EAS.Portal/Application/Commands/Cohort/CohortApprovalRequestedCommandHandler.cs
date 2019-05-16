﻿using SFA.DAS.Commitments.Api.Client.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using SFA.DAS.EAS.Portal.Application.Services;
using SFA.DAS.EAS.Portal.Types;

namespace SFA.DAS.EAS.Portal.Application.Commands.Cohort
{
    public class CohortApprovalRequestedCommandHandler : ICommandHandler<CohortApprovalRequestedCommand>
    {
        private readonly IAccountDocumentService _accountsService;
        private readonly IProviderCommitmentsApi _providerCommitmentsApi;

        public CohortApprovalRequestedCommandHandler(
            IAccountDocumentService accountsService, 
            IProviderCommitmentsApi providerCommitmentsApi)
        {
            _accountsService = accountsService;
            _providerCommitmentsApi = providerCommitmentsApi;
        }

        public async Task Handle(CohortApprovalRequestedCommand command, CancellationToken cancellationToken = default)
        {
            var accountDocumentTask = _accountsService.Get(command.AccountId);
            var commitmentTask = _providerCommitmentsApi.GetProviderCommitment(command.ProviderId, command.CommitmentId);
            await Task.WhenAll(accountDocumentTask, commitmentTask);

            var accountDocument = await accountDocumentTask;
            var account = accountDocument.Account;
            var commitment = await commitmentTask;

            var cohortReference = commitment.Reference;            
            var cohort = account.Organisations.First().Cohorts.FirstOrDefault(c => c.Id != null && c.Id.Equals(cohortReference, StringComparison.OrdinalIgnoreCase));

            if (cohort == null)
            {
                cohort = new Types.Cohort { Id = cohortReference };
                account.Organisations.First().Cohorts.Add(cohort);
            }
            
            commitment.Apprenticeships.ForEach(a =>
            {
                var apprenticeship = cohort.Apprenticeships.FirstOrDefault(ca => ca.Id == a.Id);

                if (apprenticeship == null)
                {
                    apprenticeship = new Apprenticeship { Id = a.Id};
                    cohort.Apprenticeships.Add(apprenticeship);
                }
                apprenticeship.FirstName = a.FirstName;
                apprenticeship.LastName  = a.LastName;
                apprenticeship.CourseName = a.TrainingName;
                apprenticeship.ProposedCost = a.Cost;
                apprenticeship.StartDate = a.StartDate;
                apprenticeship.EndDate = a.EndDate;
            });

            await _accountsService.Save(accountDocument);
        }
    }
}