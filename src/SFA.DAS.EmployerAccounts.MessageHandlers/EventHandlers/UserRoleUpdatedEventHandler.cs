﻿using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers
{
    public class UserRoleUpdatedEventHandler : IHandleMessages<UserJoinedEvent>
    {
        private readonly IMessagePublisher _messagePublisher;

        public UserRoleUpdatedEventHandler(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }
        public async Task Handle(UserJoinedEvent message, IMessageHandlerContext context)
        {
            await _messagePublisher.PublishAsync(
                new UserJoinedMessage(
                    message.AccountId,
                    message.UserName,
                    message.UserRef.ToString()));
        }
    }
}
