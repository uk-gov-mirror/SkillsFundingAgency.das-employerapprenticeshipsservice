using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.EAS.Portal.Client.Models;

namespace SFA.DAS.EAS.Portal.Database.Models
{
    public class AccountLegalEntity : IAccountLegalEntityDto
    {
        //todo: rename to id & name?
        [JsonProperty("accountLegalEntityId")]
        public long AccountLegalEntityId { get; private set; }

        [JsonProperty("legalEntityName")]
        public string LegalEntityName { get; private set; }
        
        [JsonProperty("reservedFundings")]
        public IEnumerable<IReservedFundingDto> ReservedFundings => _reservedFundings;

        [JsonIgnore]
        private readonly List<ReservedFunding> _reservedFundings = new List<ReservedFunding>();

        private AccountLegalEntity(long accountLegalEntityId, string legalEntityName)
        {
            AccountLegalEntityId = accountLegalEntityId;
            LegalEntityName = legalEntityName;
        }

        public AccountLegalEntity(long accountLegalEntityId, string legalEntityName,
            Guid reservationId, string courseId, string courseName, DateTime startDate, DateTime endDate)
            : this(accountLegalEntityId, legalEntityName)
        {
            AddReserveFunding(reservationId, courseId, courseName, startDate, endDate);
        }

        [JsonConstructor]
        private AccountLegalEntity()
        {
        }
        
        public void AddReserveFunding(Guid reservationId, string courseId, string courseName, DateTime startDate, DateTime endDate)
        {
            _reservedFundings.Add(new ReservedFunding(reservationId, courseId, courseName, startDate, endDate));
        }
    }
}