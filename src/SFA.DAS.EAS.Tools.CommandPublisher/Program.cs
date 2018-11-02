using System;
using NServiceBus;
using SFA.DAS.Configuration;
using SFA.DAS.EAS.Tools.CommandPublisher.DependencyResolution;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.Extensions;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.StructureMap;
using Environment = System.Environment;

namespace SFA.DAS.EAS.Tools.CommandPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = IoC.Initialize();

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.CommandPublisher")
                .UseAzureServiceBusTransport(() => container.GetInstance<EmployerFinanceConfiguration>().ServiceBusConnectionString)
                .UseLicense(container.GetInstance<EmployerFinanceConfiguration>().NServiceBusLicense.HtmlDecode())
                .UseNewtonsoftJsonSerializer()
                .UseNLogFactory()
                .UseSendOnly()
                .UseStructureMapBuilder(container);

            var endpoint = Endpoint.Start(endpointConfiguration).Result;
           
            CreateImportAccountPaymentsCommand(endpoint);
        }

        private static void CreateImportAccountPaymentsCommand(IEndpointInstance endpoint)
        {
            var accountId = GetAccountId();

            var periodEnd = GetPeriodEnd();

            SendImportAccountPaymentsCommand(endpoint, accountId, periodEnd);
        }

        private static void SendImportAccountPaymentsCommand(IEndpointInstance endpoint, int accountId, string periodEnd)
        {
            try
            {
                endpoint.Send(new ImportAccountPaymentsCommand
                {
                    AccountId = accountId,
                    PeriodEndRef = periodEnd
                });

                WriteToConsole("Command sent successfully", ConsoleColor.Green);
            }
            catch (Exception e)
            {
                WriteToConsole("Failed to send command: " + e, ConsoleColor.Red);
                throw;
            }
        }

        private static string GetPeriodEnd()
        {
            Console.Write("Which Period End do you wish to add an import payments for: ");
            var periodEnd = Console.ReadLine();
            
            return periodEnd;
        }

        private static int GetAccountId()
        {
            int accountId;

            do
            {
                Console.Write("Which Account Id do you wish to add an import payments for: ");

                if (!int.TryParse(Console.ReadLine(), out accountId))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    WriteToConsole("Invalid account Id. You must enter a number above zero" + Environment.NewLine,
                        ConsoleColor.Red);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } while (accountId == 0);

            return accountId;
        }

        private static void WriteToConsole(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor =color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }
    }
}
