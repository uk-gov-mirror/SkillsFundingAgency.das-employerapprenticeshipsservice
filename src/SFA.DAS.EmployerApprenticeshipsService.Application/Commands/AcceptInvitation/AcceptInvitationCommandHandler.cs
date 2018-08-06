using MediatR;
using SFA.DAS.Audit.Types;
using SFA.DAS.EAS.Application.Exceptions;
using SFA.DAS.Validation;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.AccountTeam;
using SFA.DAS.EAS.Domain.Models.Audit;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.NServiceBus;
using SFA.DAS.TimeProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Application.Commands.AcceptInvitation
{
    public class AcceptInvitationCommandHandler : AsyncRequestHandler<AcceptInvitationCommand>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IAuditService _auditService;
        private readonly IValidator<AcceptInvitationCommand> _validator;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILog _logger;

        public AcceptInvitationCommandHandler(IInvitationRepository invitationRepository,
            IMembershipRepository membershipRepository,
            IUserAccountRepository userAccountRepository,
            IAuditService auditService,
            IEventPublisher eventPublisher,
            IValidator<AcceptInvitationCommand> validator,
            ILog logger)
        {
            _invitationRepository = invitationRepository;
            _membershipRepository = membershipRepository;
            _userAccountRepository = userAccountRepository;
            _auditService = auditService;
            _eventPublisher = eventPublisher;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(AcceptInvitationCommand message)
        {
            _logger.Info($"Accepting Invitation '{message.Id}'");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
                throw new InvalidRequestException(validationResult.ValidationDictionary);

            var invitation = await GetInvitation(message);

            var user = await GetUser(invitation.Email);

            await CheckIfUserIsAlreadyAMember(invitation, user);

            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException("Invitation is not pending");

            if (invitation.ExpiryDate < DateTimeProvider.Current.UtcNow)
                throw new InvalidOperationException("Invitation has expired");

            await _invitationRepository.Accept(invitation.Email, invitation.AccountId, (short)invitation.RoleId);

            await CreateAuditEntry(message, user, invitation);



            await PublishUserJoinedMessage(invitation.AccountId, user);
        }

        private async Task CheckIfUserIsAlreadyAMember(Invitation invitation, User user)
        {
            var membership = await _membershipRepository.GetCaller(invitation.AccountId, user.UserRef);

            if (membership != null)
                throw new InvalidOperationException("Invited user is already a member of the Account");
        }

        private async Task<User> GetUser(string email)
        {
            var user = await _userAccountRepository.Get(email);

            if (user == null)
                throw new InvalidOperationException("Invited user was not found");

            return user;
        }

        private async Task<Invitation> GetInvitation(AcceptInvitationCommand message)
        {
            var invitation = await _invitationRepository.Get(message.Id);

            if (invitation == null)
                throw new InvalidRequestException(new Dictionary<string, string> { { "Id", "Invitation not found with given ID" } });

            return invitation;
        }

        private async Task CreateAuditEntry(AcceptInvitationCommand message, User user, Invitation existing)
        {
            await _auditService.SendAuditMessage(new EasAuditMessage
            {
                Category = "UPDATED",
                Description =
                    $"Member {user.Email} has accepted and invitation to account {existing.AccountId} as {existing.RoleId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromString("Status", InvitationStatus.Accepted.ToString())
                },
                RelatedEntities = new List<Entity>
                {
                    new Entity {Id = $"Account Id [{existing.AccountId}], User Id [{user.Id}]", Type = "Membership"}
                },
                AffectedEntity = new Entity { Type = "Invitation", Id = message.Id.ToString() }
            });
        }

        private Task PublishUserJoinedMessage(long accountId, User user)
        {
            return _eventPublisher.Publish(new UserJoinedEvent
            {
                AccountId = accountId,
                UserName = user.FullName,
                UserRef = user.Ref,
                Created = DateTime.UtcNow
            });
        }
    }
}