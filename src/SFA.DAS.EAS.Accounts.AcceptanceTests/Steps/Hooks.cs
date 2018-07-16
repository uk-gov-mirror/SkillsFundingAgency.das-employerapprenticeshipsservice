using AutoMapper;
using BoDi;
using MediatR;
using SFA.DAS.EAS.Application.DependencyResolution;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Domain.Data;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Client.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Accounts.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private readonly IContainer _container;

        public Hooks(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            
            _container = new Container(c =>
            {
                c.AddRegistry<AuditRegistry>();
                c.AddRegistry<ConfigurationRegistry>();

                //c.AddRegistry<EventsRegistry>();
                c.For<IEventsApi>().Use(objectContext.EventsApiMock.Object);

                c.AddRegistry<HashingRegistry>();
                c.AddRegistry<LoggerRegistry>();
                c.AddRegistry<LevyRegistry>();
                c.AddRegistry<MapperRegistry>();
                c.AddRegistry<MessagePublisherRegistry>();
                c.AddRegistry<MediatorRegistry>();
                c.AddRegistry<RepositoriesRegistry>();
                c.AddRegistry<RepositoriesRegistry>();
                c.Scan(scanner =>
                {
                    scanner.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scanner.RegisterConcreteTypesAgainstTheFirstInterface();
                });
            });
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _objectContainer.RegisterInstanceAs(_container);
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IEventsApi>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IEventsApiClientConfiguration>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IMapper>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IMediator>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPayeRepository>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPublicHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<ILog>());

            _objectContainer.RegisterInstanceAs(_container.GetInstance<IMultiVariantTestingService>());

            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerAccountDbContext>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerFinancialDbContext>());
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _container.Dispose();
        }
    }
}