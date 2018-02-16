﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Domain.Data.Repositories;

namespace SFA.DAS.EAS.Application.Commands.RejectTransferConnectionInvitation
{
    public class RejectTransferConnectionInvitationCommandHandler : AsyncRequestHandler<RejectTransferConnectionInvitationCommand>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly ITransferConnectionInvitationRepository _transferConnectionInvitationRepository;
        private readonly IUserRepository _userRepository;

        public RejectTransferConnectionInvitationCommandHandler(
            IEmployerAccountRepository employerAccountRepository,
            ITransferConnectionInvitationRepository transferConnectionInvitationRepository,
            IUserRepository userRepository)
        {
            _employerAccountRepository = employerAccountRepository;
            _transferConnectionInvitationRepository = transferConnectionInvitationRepository;
            _userRepository = userRepository;
        }

        protected override async Task HandleCore(RejectTransferConnectionInvitationCommand message)
        {
            var rejectorAccount = await _employerAccountRepository.GetAccountById(message.AccountId.Value);
            var rejectorUser = await _userRepository.GetUserById(message.UserId.Value);
            var transferConnectionInvitation = await _transferConnectionInvitationRepository.GetTransferConnectionInvitationToApproveOrReject(message.TransferConnectionInvitationId.Value, rejectorAccount.Id);

            transferConnectionInvitation.Reject(rejectorAccount, rejectorUser);
        }
    }
}