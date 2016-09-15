﻿namespace SFA.DAS.Commitments.Api.Types
{
    public class CommitmentListItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long EmployerAccountId { get; set; }
        public string EmployerAccountName { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public CommitmentStatus Status { get; set; }

        public bool CanBeSubmitted()
        {
            return Status == CommitmentStatus.Draft;
        }
    }
}