using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Portal.Application.Services;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.EAS.Portal.Worker.EventHandlers.PayeScheme
{
    public class AddedPayeSchemeEventHandler : EventHandler<AddedPayeSchemeEvent>
    {

        public AddedPayeSchemeEventHandler(
            IAccountDocumentService accountService,
            IMessageContextInitialisation messageContextInitialisation,
            ILogger<AddedPayeSchemeEventHandler> logger)
                : base(accountService,messageContextInitialisation,logger)
        {

        }

        protected override async Task Handle(AddedPayeSchemeEvent command, CancellationToken cancellationToken = default)
        {
            var accountDoc = await GetOrCreateAccountDocument(command.AccountId, cancellationToken);

            var existingPaye = accountDoc.Account.PayeSchemes.FirstOrDefault(paye => paye.PayeRef == command.PayeRef);

            if (existingPaye == null)
            {
                accountDoc.Account.PayeSchemes.Add(new Client.Types.PayeScheme { PayeRef = command.PayeRef });
                await AccountDocumentService.Save(accountDoc, cancellationToken);
            }
        }
    }
}
