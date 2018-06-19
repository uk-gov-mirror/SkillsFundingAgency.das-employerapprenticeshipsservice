using BoDi;
using SFA.DAS.EAS.Application.DependencyResolution;
using SFA.DAS.EAS.Application.Hashing;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.HashingService;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Finance.AcceptanceTests.Steps
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private readonly IContainer _container;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            _container = new Container(c =>
            {
                c.AddRegistry<ConfigurationRegistry>();
                c.AddRegistry<HashingRegistry>();
                c.AddRegistry<MediatorRegistry>();
                c.AddRegistry<PaymentsRegistry>();
            });
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _objectContainer.RegisterInstanceAs(_container);
            _objectContainer.RegisterInstanceAs(_container.GetInstance<EmployerAccountDbContext>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IHashingService>());
            _objectContainer.RegisterInstanceAs(_container.GetInstance<IPublicHashingService>());
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _container.Dispose();
        }
    }
}