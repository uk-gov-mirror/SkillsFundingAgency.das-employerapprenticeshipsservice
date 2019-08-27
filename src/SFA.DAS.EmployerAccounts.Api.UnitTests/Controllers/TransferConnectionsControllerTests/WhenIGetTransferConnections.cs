﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetTransferConnections;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.TransferConnectionsControllerTests
{
    [TestFixture]
    public class WhenIGetTransferConnections
    {
        private TransferConnectionsController _controller;
        private Mock<IMediator> _mediator;
        private GetTransferConnectionsQuery _query;
        private GetTransferConnectionsResponse _response;
        private IEnumerable<TransferConnection> _transferConnections;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _query = new GetTransferConnectionsQuery();
            _transferConnections = new List<TransferConnection>();
            _response = new GetTransferConnectionsResponse { TransferConnections = _transferConnections };

            _mediator.Setup(m => m.SendAsync(_query)).ReturnsAsync(_response);

            _controller = new TransferConnectionsController(_mediator.Object);
        }

        [Test]
        public async Task ThenAGetTransferConnectionsQueryShouldBeSent()
        {
            await _controller.GetTransferConnections(_query);

            _mediator.Verify(m => m.SendAsync(_query), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnTransferConnections()
        {
            var result = await _controller.GetTransferConnections(_query) as OkNegotiatedContentResult<IEnumerable<TransferConnection>>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.SameAs(_transferConnections));
        }
    }
}