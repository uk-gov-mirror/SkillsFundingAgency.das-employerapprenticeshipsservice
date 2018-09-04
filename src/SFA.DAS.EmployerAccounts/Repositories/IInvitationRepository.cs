﻿using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Repositories
{
    public interface IInvitationRepository
    {
        Task<List<InvitationView>> Get(string userId);
        Task<InvitationView> GetView(long id);
        Task<long> Create(Invitation invitation);
        Task<Invitation> Get(long id);
        Task<Invitation> Get(long accountId, string email);
        Task ChangeStatus(Invitation invitation);
        Task Resend(Invitation invitation);
        Task Accept(string email, long accountId, short roleId);
        Task<int> GetNumberOfInvites(string userId);
    }
}