using Moq;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.NLog.Logger;
using StructureMap;
using SFA.DAS.HashingService;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.EmployerAccounts.TestCommon.DependencyResolution
{
    using SFA.DAS.EmployerFinance.Services;

    using IHmrcService = SFA.DAS.EmployerAccounts.Interfaces.IHmrcService;

    public class LevyWorkerDefaultRegistry : Registry
    {
        public LevyWorkerDefaultRegistry(Interfaces.IHmrcService hmrcService, Mock<IMessagePublisher> messagePublisher, Mock<IMessageSubscriberFactory> messageSubscriberFactory, IEventsApi eventApi = null)
        {
            Scan(s =>
            {
                s.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                s.RegisterConcreteTypesAgainstTheFirstInterface();
            });
            
            For<IEventsApi>().Use(eventApi ?? Mock.Of<IEventsApi>()); 
            For<IHmrcService>().Use(hmrcService);
            For<IHmrcDateService>().Use<HmrcDateService>();
            For<IHashingService>().Use(new HashingService.HashingService("12345QWERTYUIOPNDGHAK", "TEST: Dummy hash code London is a city in UK"));
            For<ILog>().Use(Mock.Of<ILog>());
            For<IMessagePublisher>().Use(messagePublisher.Object);
            For<IMessageSubscriberFactory>().Use(messageSubscriberFactory.Object);
        }
    }
}