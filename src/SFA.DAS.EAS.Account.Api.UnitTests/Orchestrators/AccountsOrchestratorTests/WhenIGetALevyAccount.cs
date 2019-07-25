﻿using AutoMapper;
using Castle.Core.Internal;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Orchestrators;
using SFA.DAS.EAS.Application.Queries.GetEmployerAccountByHashedId;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SFA.DAS.EAS.Domain.Models.Transfers;
using SFA.DAS.EAS.Application.Queries.GetTransferAllowance;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetAccountBalances;
using System.Collections.Generic;

namespace SFA.DAS.EAS.Account.Api.UnitTests.Orchestrators.AccountsOrchestratorTests
{
    internal class WhenIGetALevyAccount : AccountsOrchestratorTestsBase
    {
        private AccountsOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;
        private Mock<ILog> _log;
        private Mock<IHashingService> _hashingService;
        private IMapper _mapper;
        private TransferAllowance _transferAllowance;
        private const decimal AccountBalance = 678.90M;

        [SetUp]
        public void Arrange()
        {
            _transferAllowance = new TransferAllowance { RemainingTransferAllowance = 123.45M, StartingTransferAllowance = 234.56M };
            _mediator = new Mock<IMediator>();
            _mapper = ConfigureMapper();
            _log = new Mock<ILog>();
            _hashingService = new Mock<IHashingService>();
            _orchestrator = new AccountsOrchestrator(_mediator.Object, _log.Object, _mapper, _hashingService.Object);

            var response = new GetEmployerAccountByHashedIdResponse
            {
                Account = new AccountDetail
                {
                    AccountAgreementTypes = new List<string>()
                    {
                        "Levy"
                    }
                }
            };

            _mediator
                .Setup(x => x.SendAsync(It.IsAny<GetEmployerAccountByHashedIdQuery>()))
                .ReturnsAsync(response)
                .Verifiable("Get account was not called");

            _mediator
                .Setup(x => x.SendAsync(It.IsAny<GetTransferAllowanceQuery>()))
                .ReturnsAsync(new GetTransferAllowanceResponse { TransferAllowance = _transferAllowance })
                .Verifiable("Get transfer balance was not called");

            _mediator
                .Setup(x => x.SendAsync(It.IsAny<GetAccountBalancesRequest>()))
                .ReturnsAsync(new GetAccountBalancesResponse
                {
                    Accounts = new List<AccountBalance> { new AccountBalance { Balance = AccountBalance } }
                })
                .Verifiable("Get account balance was not called");
        }

        [Test]
        public async Task ThenResponseShouldHaveAccountAgreementTypeSetToLevy()
        {
            //Arrange
            string agreementType = "Levy";
            const string hashedAgreementId = "ABC123";

            //Act
            var result = await _orchestrator.GetAccount(hashedAgreementId);

            //Assert
            Assert.AreEqual(agreementType, result.Data.AccountAgreementType);
        }        
    }
}
