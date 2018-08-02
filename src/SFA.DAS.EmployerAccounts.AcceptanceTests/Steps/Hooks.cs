using System.Data.Common;
using BoDi;
using NServiceBus;
using SFA.DAS.EAS.Application.DependencyResolution;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.Authentication;
using SFA.DAS.EAS.Infrastructure.Authorization;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.Infrastructure.NServiceBus;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.TestCommon.Extensions;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.HashingService;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.EntityFramework;
using SFA.DAS.NServiceBus.MsSqlServer;
using SFA.DAS.NServiceBus.NewtonsoftSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.StructureMap;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerAccounts.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly ObjectContext _objectContext;
        private readonly IObjectContainer _objectContainer;
        private readonly IContainer _container;

        private IEndpointInstance _nServiceBusEndpoint;

        public Hooks(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContext = objectContext;
            _objectContainer = objectContainer;

            _container = ConfigureForTests();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            SetInitialisedDbContext();
            StartServiceBusEndpoint();
            SetupTestContainer();
            SetupTestContext();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _container.GetInstance<EmployerAccountsDbContext>().Database.CurrentTransaction.Rollback();
            _container.GetInstance<EmployerFinanceDbContext>().Database.CurrentTransaction.Rollback();
            //_container.GetInstance<EmployerAccountsDbContext>().Database.CurrentTransaction.Commit();
            //_container.GetInstance<EmployerFinanceDbContext>().Database.CurrentTransaction.Commit();

            StopServiceBusEndpoint();

            _objectContainer.Dispose();
            _container.Dispose();
        }

        public IContainer ConfigureForTests()
        {
            return new Container(c =>
            {
                c.AddRegistry<AuditRegistry>();
                c.AddRegistry<ConfigurationRegistry>();
                c.AddRegistry<HashingRegistry>();
                c.AddRegistry<LoggerRegistry>();
                c.AddRegistry<MapperRegistry>();
                c.AddRegistry<MessagePublisherRegistry>();
                c.AddRegistry<MediatorRegistry>();
                c.AddRegistry<RepositoriesRegistry>();

                c.AddRegistry<EmployerAccountsDbContextRegistry>();
                c.AddRegistry<EmployerFinanceDbContextRegistry>();

                c.AddRegistry<NServiceBusRegistry>();
                c.For<IEventPublisher>().Use<EventPublisher>();

                c.Scan(scanner =>
                {
                    scanner.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scanner.RegisterConcreteTypesAgainstTheFirstInterface();
                });
            });
        }

        private void SetupTestContainer()
        {
            _container.Configure(c =>
            {
                c.For<IAuthenticationService>().Use(_objectContext.AuthenticationService.Object);
                c.For<IAuthorizationService>().Use(_objectContext.AuthorizationService.Object);
                c.For<ICookieStorageService<EmployerAccountData>>().Use(_objectContext.CookieEmployerAccountDataStorageService.Object);
                c.For<ICookieStorageService<FlashMessageViewModel>>().Use(_objectContext.CookieFlashMessageViewModelStorageService.Object);
                c.For<IEventsApi>().Use(_objectContext.EventsApiMock.Object);
                c.For<IMessageSession>().Use(_objectContext.NServiceBusEndpoint);
            });

            _objectContainer.RegisterInstanceAs(_container.GetInstance<IHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPublicHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPayeRepository>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IEventPublisher>());

            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerAccountsDbContext>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerFinanceDbContext>());
        }

        private void SetupTestContext()
        {
            _objectContext
                .SetupEapOrchestrator(_container)
                .SetupEapController(_container)
                .EmployerAccountsDbContextBeginTransaction(_container)
                .EmployerFinanceDbContextBeginTransaction(_container);
        }

        private void SetInitialisedDbContext()
        {
            _container.Configure(c =>
            {
                c.For<EmployerAccountsDbContext>().Use(_container.GetInstance<EmployerAccountsDbContext>());
                c.For<EmployerFinanceDbContext>().Use(_container.GetInstance<EmployerFinanceDbContext>());
            });
        }

        private void StartServiceBusEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EAS.Web")
                .SetupAzureServiceBusTransport(_container.GetInstance<EmployerApprenticeshipsServiceConfiguration>().ServiceBusConnectionString)
                .SetupEntityFrameworkUnitOfWork<EmployerAccountsDbContext>()
                .SetupErrorQueue()
                .SetupHeartbeat()
                .SetupInstallers()
                .SetupMetrics()
                .SetupMsSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .SetupNewtonsoftSerializer()
                .SetupNLogFactory()
                .SetupOutbox()
                .SetupStructureMapBuilder(_container);

            _nServiceBusEndpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            _objectContext.NServiceBusEndpoint = _nServiceBusEndpoint;
        }

        private void StopServiceBusEndpoint()
        {
            _nServiceBusEndpoint?.Stop().GetAwaiter().GetResult();
        }
    }
}