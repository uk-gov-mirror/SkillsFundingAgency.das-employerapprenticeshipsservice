using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EAS.Portal.Application.Services;
using SFA.DAS.EAS.Portal.Client.Database.Models;
using SFA.DAS.EAS.Portal.Client.Types;
using SFA.DAS.EAS.Portal.Worker.EventHandlers.PayeScheme;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.EAS.Portal.UnitTests.Worker.EventHandlers.Paye
{
    [Parallelizable]
    [TestFixture]
    class WhenPayeSchemeAddedTests
    {
        AddedPayeSchemeEventHandler _sut;
        Mock<IAccountDocumentService> _accountServiceMock;
        Mock<IMessageContextInitialisation> _messageMock;
        private Mock<ILogger<AddedPayeSchemeEventHandler>> _loggerMock;
        AccountDocument _accountDoc;
        IFixture Fixture = new Fixture();
        private Mock<IMessageHandlerContext> MessageHandlerContext;

        [SetUp]
        public void SetUp()
        {
            _accountServiceMock = new Mock<IAccountDocumentService>();
            _loggerMock = new Mock<ILogger<AddedPayeSchemeEventHandler>>();
            _messageMock = new Mock<IMessageContextInitialisation>();
            _sut = new AddedPayeSchemeEventHandler(_accountServiceMock.Object,_messageMock.Object,_loggerMock.Object);

            Fixture.Customize<Account>(a => a
                .With(acc => acc.Id, 1));
            _accountDoc = Fixture
                .Build<AccountDocument>()
                .Create();

            _accountServiceMock.Setup(mock => mock.Get(1, It.IsAny<CancellationToken>())).ReturnsAsync(_accountDoc);

            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            MessageHandlerContext.Setup(c => c.MessageId).Returns("ThisIsAMessage");

            var messageHeaders = new Mock<IReadOnlyDictionary<string, string>>();
            messageHeaders.SetupGet(c => c["NServiceBus.TimeSent"]).Returns(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:ffffff Z", CultureInfo.InvariantCulture));
            MessageHandlerContext.Setup(c => c.MessageHeaders).Returns(messageHeaders.Object);
        }

        [Test]
        [Category("UnitTest")]
        public async Task ShouldGetAccount()
        {
            // Arrange
            var userRef = Guid.NewGuid();
            var payeCommand = new AddedPayeSchemeEvent{
                AccountId = 1,
                UserName = "Bob",
                UserRef = userRef,
                PayeRef = "payepayepaye" };


            // Act
            await _sut.Handle(payeCommand,MessageHandlerContext.Object);

            // Assert
            _accountServiceMock.VerifyAll();
        }

        [Test]
        [TestCase(0, Description = "No existing PAYE schemes")]
        [TestCase(2, Description = "Multiple existing PAYE, not same")]
        [Category("UnitTest")]
        public async Task ShouldAddPayeSchemeToAccountWhenNotExists(int numOfExistingPayeSchemes)
        {
            // Arrange
            _accountDoc.Account.PayeSchemes = Fixture.CreateMany<PayeScheme>(numOfExistingPayeSchemes).ToList();
            var userRef = Guid.NewGuid();
            var payeCommand = new AddedPayeSchemeEvent
            {
                AccountId = 1,
                UserName = "Bob",
                UserRef = userRef,
                PayeRef = "payepayepaye"
            };

            AccountDocument resultAccountDoc = null;
            _accountServiceMock.Setup(mock => mock.Save(It.Is<AccountDocument>(ad => ad.AccountId == 1), It.IsAny<CancellationToken>()))
                .Callback((AccountDocument accountDoc, CancellationToken ct) =>
                {
                    resultAccountDoc = accountDoc;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.Handle(payeCommand,MessageHandlerContext.Object);

            // Assert
            resultAccountDoc.Account.PayeSchemes.Count.Should().Be(numOfExistingPayeSchemes + 1);
        }

        [Test]
        [Category("UnitTest")]
        public async Task ShouldNotUpdateAccountIfAlreadyExists()
        {
            // Arrange
            var existingPaye = new PayeScheme { PayeRef = "payepayepaye" };
            _accountDoc.Account.PayeSchemes = Fixture.CreateMany<PayeScheme>(3).ToList();
            _accountDoc.Account.PayeSchemes.Add(existingPaye);
            var userRef = Guid.NewGuid();
            var payeCommand = new AddedPayeSchemeEvent
            {
                AccountId = 1,
                UserName = "Bob",
                UserRef = userRef,
                PayeRef = "payepayepaye"
            };

            // Act
            await _sut.Handle(payeCommand,MessageHandlerContext.Object);

            // Assert
            _accountServiceMock.Verify(mock => mock.Save(It.IsAny<AccountDocument>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
