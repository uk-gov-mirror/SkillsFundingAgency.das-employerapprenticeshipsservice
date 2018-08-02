using System.Data.Common;
using BoDi;
using HMRC.ESFA.Levy.Api.Client;
using MediatR;
using Moq;
using NServiceBus;
using SFA.DAS.EAS.Application.DependencyResolution;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.Authentication;
using SFA.DAS.EAS.Infrastructure.Authorization;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.Infrastructure.NServiceBus;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.TestCommon.Extensions;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.EAS.TestCommon.TestRepositories;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.EntityFramework;
using SFA.DAS.NServiceBus.MsSqlServer;
using SFA.DAS.NServiceBus.NewtonsoftSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.EAS.Web.ViewModels;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerFinance.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks 
    {
        private readonly ObjectContext _objectContext;
        private readonly IObjectContainer _objectContainer;
        private readonly IContainer _container;

        private IEndpointInstance _nServiceBusEndpoint;
        private IEndpointInstance _financeMessageHandlersEndPoint;
        private IEndpointInstance _financeJobsEndPoint;

        private readonly Mock<ICurrentDateTime> _currentDateTimeMock;

        public Hooks(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContext = objectContext;
            _objectContainer = objectContainer;

            _currentDateTimeMock = new Mock<ICurrentDateTime>();

            _container = ConfigureForTests();
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            SetInitialisedDbContext();
            ReplaceTestInterfaces();
            StartEndPoints();
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
            StopEmployerFinanceMessageHandlerEndPoint();
            StopEmployerFinanceJobsEndPoint();

            _objectContainer.Dispose();
            _container.Dispose();
        }

        public IContainer ConfigureForTests()
        {
            return new Container(c =>
            {
                c.AddRegistry<ApprenticeshipLevyRegistry>();
                c.AddRegistry<AuditRegistry>();
                c.AddRegistry<CachesRegistry>();
                c.AddRegistry<CommitmentsRegistry>();
                c.AddRegistry<ConfigurationRegistry>();
                c.AddRegistry<DataRegistry>();
                c.AddRegistry<EventsRegistry>();
                c.AddRegistry<ExecutionPoliciesRegistry>();
                c.AddRegistry<HashingRegistry>();
                c.AddRegistry<LoggerRegistry>();
                c.AddRegistry<MapperRegistry>();
                c.AddRegistry<MediatorRegistry>();
                c.AddRegistry<MessagePublisherRegistry>();
                c.AddRegistry<NotificationsRegistry>();
                c.AddRegistry<RepositoriesRegistry>();
                c.AddRegistry<PaymentsRegistry>();
                c.AddRegistry<TokenServiceRegistry>();
                c.AddRegistry<MessageHandlers.DependencyResolution.DefaultRegistry>();


                c.AddRegistry<EmployerAccountsDbContextRegistry>();
                c.AddRegistry<EmployerFinanceDbContextRegistry>();

                c.AddRegistry<NServiceBusRegistry>();
                c.Scan(scanner =>
                {
                    scanner.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scanner.RegisterConcreteTypesAgainstTheFirstInterface();
                });

                c.For<IEventPublisher>().Use<EventPublisher>();
            });
        }

        private void StartEndPoints()
        {
            StartServiceBusEndpoint();
            StartEmployerFinanceMessageHandlersEndPoint();
            StartEmployerFinanceJobsEndPoint();
        }

        private void SetupTestContainer()
        {
            _container.Configure(c =>
            {
                c.For<IApprenticeshipLevyApiClient>().Use(_objectContext.ApprenticeshipLevyApiClient.Object);
                c.For<IAuthenticationService>().Use(_objectContext.AuthenticationService.Object);
                c.For<IAuthorizationService>().Use(_objectContext.AuthorizationService.Object);
                c.For<ICookieStorageService<EmployerAccountData>>().Use(_objectContext.CookieEmployerAccountDataStorageService.Object);
                c.For<ICookieStorageService<FlashMessageViewModel>>().Use(_objectContext.CookieFlashMessageViewModelStorageService.Object);
                c.For<ICurrentDateTime>().Use(_currentDateTimeMock.Object);
                c.For<IEventsApi>().Use(_objectContext.EventsApiMock.Object);
                c.For<IMessageSession>().Use(_objectContext.NServiceBusEndpoint);
                c.For<ITestTransactionRepository>().Use<TestTransactionRepository>();
            });

            _objectContainer.RegisterInstanceAs(_container.GetInstance<IHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IMediator>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPublicHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<ILog>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<ITransactionRepository>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<ITestTransactionRepository>());

            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerAccountsDbContext>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerFinanceDbContext>());
        }


        private void SetupTestContext()
        {
            _objectContext
                .SetupEatOrchestrator(_container)
                .SetupEatController(_container)
                .SetupEapOrchestrator(_container)
                .SetupEapController(_container)
                .EmployerAccountsDbContextBeginTransaction(_container)
                .EmployerFinanceDbContextBeginTransaction(_container);
        }

        private void ReplaceTestInterfaces()
        {
            _container.Configure(c =>
            {
                c.For<IApprenticeshipLevyApiClient>().Use(_objectContext.ApprenticeshipLevyApiClient.Object);
                c.For<IEventsApi>().Use(_objectContext.EventsApiMock.Object);
                c.For<IAuthenticationService>().Use(_objectContext.AuthenticationService.Object);
            });
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
        
        private void StartEmployerFinanceMessageHandlersEndPoint()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.MessageHandlers")
                .SetupAzureServiceBusTransport(_container.GetInstance<EmployerApprenticeshipsServiceConfiguration>().ServiceBusConnectionString)
                .SetupEntityFrameworkUnitOfWork<EmployerFinanceDbContext>()
                .SetupErrorQueue()
                .SetupInstallers()
                //.SetupLicense(container.GetInstance<EmployerApprenticeshipsServiceConfiguration>().NServiceBusLicense)
                .SetupMsSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .SetupNewtonsoftSerializer()
                .SetupNLogFactory()
                .SetupOutbox()
                .SetupStructureMapBuilder(_container);

            // Clear any existing
            endpointConfiguration.PurgeOnStartup(true);

            _financeMessageHandlersEndPoint = Endpoint.Start(endpointConfiguration).ConfigureAwait(false).GetAwaiter().GetResult();

            _objectContext.FinanceMessageHandlersEndPoint = _financeMessageHandlersEndPoint;
        }

        private void StopEmployerFinanceMessageHandlerEndPoint()
        {
            _financeMessageHandlersEndPoint?.Stop().GetAwaiter().GetResult();
        }

        private void StartEmployerFinanceJobsEndPoint()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinance.Jobs")
                .SetupAzureServiceBusTransport(_container.GetInstance<EmployerApprenticeshipsServiceConfiguration>().ServiceBusConnectionString)
                //.SetupLicense(container.GetInstance<EmployerApprenticeshipsServiceConfiguration>().NServiceBusLicense)
                .SetupMsSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .SetupNewtonsoftSerializer()
                .SetupNLogFactory()
                .SetupSendOnly()
                .SetupStructureMapBuilder(_container);

            _financeJobsEndPoint = Endpoint.Start(endpointConfiguration).ConfigureAwait(false).GetAwaiter().GetResult();

            _objectContext.FinanceJobsEndPoint = _financeJobsEndPoint;
        }

        private void StopEmployerFinanceJobsEndPoint()
        {
            _financeMessageHandlersEndPoint?.Stop().GetAwaiter().GetResult();
        }
    }
}
