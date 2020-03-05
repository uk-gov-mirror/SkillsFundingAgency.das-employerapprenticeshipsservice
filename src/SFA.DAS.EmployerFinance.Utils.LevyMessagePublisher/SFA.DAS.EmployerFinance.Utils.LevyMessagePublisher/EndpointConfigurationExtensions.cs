using System;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;

namespace SFA.DAS.EmployerFinance.Utils.LevyMessagePublisher
{
    public static class EndpointConfigurationExtensions
    {
        public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder)
        {
            config.UseAzureServiceBusTransport(connectionStringBuilder(), ConfigureRouting);
            return config;
        }

        private static void ConfigureRouting(RoutingSettings routing)
        {
            routing.RouteToEndpoint(
                typeof(ImportLevyDeclarationsCommand).Assembly,
                typeof(ImportLevyDeclarationsCommand).Namespace,
                "SFA.DAS.EmployerFinance.MessageHandlers"
            );
        }
    }
}
