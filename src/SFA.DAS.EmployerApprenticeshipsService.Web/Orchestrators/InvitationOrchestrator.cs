﻿using System;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Application;
using SFA.DAS.EmployerApprenticeshipsService.Application.Commands.AcceptInvitation;
using SFA.DAS.EmployerApprenticeshipsService.Application.Commands.CreateInvitation;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetInvitation;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators
{
    public class InvitationOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public InvitationOrchestrator(IMediator mediator, ILogger logger)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<InvitationView> GetInvitation(long id)
        {
            var response = await _mediator.SendAsync(new GetInvitationRequest
            {
                Id = id
            });

            return response.Invitation;
        }

        public async Task AcceptInvitation(long invitationId, string externalUserId)
        {
            await _mediator.SendAsync(new AcceptInvitationCommand
            {
                Id = invitationId,
                ExternalUserId = externalUserId
            });
        }

        public async Task CreateInvitation(InviteTeamMemberViewModel model, string externalUserId)
        {
            try
            {
                await _mediator.SendAsync(new CreateInvitationCommand
                {
                    AccountId = model.AccountId,
                    ExternalUserId = externalUserId,
                    Name = model.Name,
                    Email = model.Email,
                    RoleId = model.Role
                });
            }
            catch (InvalidRequestException ex)
            {
                _logger.Info(ex);
            }
            
        }
    }
}