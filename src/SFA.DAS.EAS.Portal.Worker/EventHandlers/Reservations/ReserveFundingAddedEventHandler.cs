﻿using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EAS.Portal.Application.Commands;
using SFA.DAS.EAS.Portal.Events.Reservations;

namespace SFA.DAS.EAS.Portal.Worker.EventHandlers.Reservations
{
    //todo: rename when know real event name
    class ReserveFundingAddedEventHandler : IHandleMessages<ReserveFundingAddedEvent>
    {
        // NServiceBus can't inject an interface message with methods
        private readonly AddReserveFundingCommand _addReserveFundingCommand;

        public ReserveFundingAddedEventHandler(AddReserveFundingCommand addReserveFundingCommand)
        {
            _addReserveFundingCommand = addReserveFundingCommand;
        }

        public Task Handle(ReserveFundingAddedEvent message, IMessageHandlerContext context)
        {
            return _addReserveFundingCommand.Execute(message, context.MessageId);
        }
    }
}
