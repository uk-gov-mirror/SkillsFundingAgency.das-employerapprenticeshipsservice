﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EAS.Portal.Configuration;
using SFA.DAS.EAS.Portal.Database;

namespace SFA.DAS.EAS.Portal.Worker.Startup
{
    public static class CosmosDatabaseStartup
    {
        public static IServiceCollection AddCosmosDatabase(this IServiceCollection services)
        {
            //todo: better way to get at config?
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            return services
                .AddTransient<IDocumentClientFactory, DocumentClientFactory>()
                .AddTransient(sp => sp.GetService<IDocumentClientFactory>().CreateDocumentClient())
                .Configure<CosmosDatabaseConfiguration>(configuration.GetPortalSection("CosmosDatabase"));
        }
    }
}
