using System;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;

namespace SFA.DAS.EmployerFinance.Utils.LevyMessagePublisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please select an option: ");
            Console.WriteLine("1: Pull levy for account/paye scheme");
            var selectedOption = Console.ReadKey().KeyChar;

            switch (selectedOption)
            {
                case '1':
                    await PullLevyForAccountAndScheme();
                    break;
                default:
                    Console.WriteLine("Invalid option. Terminating immediately to teach a valuable lesson.");
                    Console.ReadKey();
                    break;
            }
        }

        private static async Task PullLevyForAccountAndScheme()
        {
            Console.Clear();
            Console.WriteLine("Enter the account id:");
            var accountIdString = Console.ReadLine();
            long accountId;
            if (!Int64.TryParse(accountIdString, out accountId))
            {
                Console.WriteLine("Account id must be a long. Terminating immediately to teach a valuable lesson.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Enter the paye scheme:");
            var payeScheme = Console.ReadLine();

            var message = new ImportLevyDeclarationsCommand();
            var messageSession = await GetNServiceBusMessageSession();
            await messageSession.Send(message);
            Console.WriteLine("Message published successfully. The app will now exit.");
            Console.ReadLine();
        }

        private static async Task<IMessageSession> GetNServiceBusMessageSession()
        {
            Console.WriteLine("Enter the service bus connection string:");
            var serviceBusConnectionString = Console.ReadLine();
            
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.Jobs")
                    .UseErrorQueue("SFA.DAS.EmployerFinance.Jobs-errors")
                    .UseAzureServiceBusTransport(() => serviceBusConnectionString)
                    .UseNewtonsoftJsonSerializer()
                    .UseNLogFactory()
                    .UseSendOnly();

            return await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        }
    }
}
