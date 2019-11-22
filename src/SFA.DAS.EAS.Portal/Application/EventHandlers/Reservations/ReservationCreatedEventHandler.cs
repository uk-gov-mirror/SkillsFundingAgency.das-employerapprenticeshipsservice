using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Portal.Application.Services.AccountDocumentService;
using SFA.DAS.EAS.Portal.TypesExtensions;
using SFA.DAS.Reservations.Messages;

namespace SFA.DAS.EAS.Portal.Application.EventHandlers.Reservations
{
    public class ReservationCreatedEventHandler : IEventHandler<ReservationCreatedEvent>
    {
        private readonly IAccountDocumentService _accountDocumentService;
        private readonly ILogger<ReservationCreatedEventHandler> _logger;

        public ReservationCreatedEventHandler(
            IAccountDocumentService accountDocumentService,
            ILogger<ReservationCreatedEventHandler> logger)
        {
            _accountDocumentService = accountDocumentService;
            _logger = logger;
        }

        public async Task Handle(ReservationCreatedEvent reservationCreatedEvent, CancellationToken cancellationToken = default)
        {
            var accountDocument = await _accountDocumentService.GetOrCreate(reservationCreatedEvent.AccountId, cancellationToken);
            bool isExistingReservation = false;
            var organisation = accountDocument.Account.GetOrAddOrganisation(reservationCreatedEvent.AccountLegalEntityId,
                addedOrganisation => addedOrganisation.Name = reservationCreatedEvent.AccountLegalEntityName,
                existingOrganisation =>
                {
                    isExistingReservation = existingOrganisation.Reservations.Any(r => r.Id.Equals(reservationCreatedEvent.Id));
                });

            if (!isExistingReservation)
            {
                organisation.Reservations.Add(new Client.Types.Reservation
                {
                    Id = reservationCreatedEvent.Id,
                    CourseCode = reservationCreatedEvent.CourseId,
                    CourseName = reservationCreatedEvent.CourseName,
                    StartDate = reservationCreatedEvent.StartDate,
                    EndDate = reservationCreatedEvent.EndDate
                });

                await _accountDocumentService.Save(accountDocument, cancellationToken);
            }
        }
    }
}