﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.EmployerAgreement;
using SFA.DAS.EAS.Domain.Models.Organisation;

namespace SFA.DAS.EAS.Domain.Data.Repositories
{
    public interface IEmployerAgreementRepository
    {
        [Obsolete("This method has been replaced by the GetAccountEmployerAgreementsQueryHandler query")]
        Task<List<AccountSpecificLegalEntity>> GetLegalEntitiesLinkedToAccount(long accountId, bool signedOnly);
        Task<EmployerAgreementView> GetEmployerAgreement(long agreementId);
        Task SignAgreement(SignEmployerAgreement agreement);
        Task<EmployerAgreementTemplate> GetEmployerAgreementTemplate(int templateId);
        Task RemoveLegalEntityFromAccount(long agreementId);
        Task EvaluateEmployerLegalEntityAgreementStatus(long accountId, long legalEntityId);
        Task<AccountLegalEntityModel> GetAccountLegalEntity(long accountLegalEntityId);
    }
}