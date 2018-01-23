using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Api;
using SFA.DAS.EAS.Api.DependencyResolution;
using SFA.DAS.EAS.Domain.Data.Repositories;

namespace SFA.DAS.EAS.Account.Api.Tests
{
    [TestFixture]
    public class Class1
    {
        private TestServer server;
        private IAccountApiClient client;

        [Test]
        public async Task Should()
        {
            var mockRepo = new Mock<IDasLevyRepository>();
            //mockRepo.Setup(x => x.)


            var container = IocConfig.Container;
            container.EjectAllInstancesOf<IDasLevyRepository>();
            container.Configure(x =>
            {
                x.For<IDasLevyRepository>().Use(mockRepo.Object);
            });
            var pageOfAccounts = await client.GetPageOfAccounts(1, 10, null);
            Console.WriteLine(JsonConvert.SerializeObject(pageOfAccounts));
        } 

        [SetUp]
        public void Setup()
        {
            var process = Process.Start(@"C:\Program Files\Microsoft SDKs\Azure\Emulator\csrun", "/devstore");
            if (process != null)
            {
                process.WaitForExit();
            }
            else
            {
                throw new Exception("Unable to start storage emulator.");
            }
            server = TestServer.Create<Startup>();
            client = new AccountApiClient2(server.HttpClient);
        }

        [TearDown]
        public void TearDown()
        {
            server.Dispose();
        }

    }
}
