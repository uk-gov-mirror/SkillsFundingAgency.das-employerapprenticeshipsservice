using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using BoDi;
using MediatR;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.Levy;
using SFA.DAS.EAS.Domain.Models.Payments;
using SFA.DAS.EAS.Domain.Models.TransferConnections;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.TestCommon.Extensions;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.ViewModels.TransferConnectionInvitations;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Accounts.AcceptanceTests.Steps
{
    [Binding]
    public class TransferConnectionInvitationSteps
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ObjectContext _objectContext;

        private readonly TransferConnectionInvitationsController _controller;
        private readonly IDasLevyRepository _dasLevyRepository;

        private ActionResult _actionResult;

        public TransferConnectionInvitationSteps(IObjectContainer objectContainer, ObjectContext objectContext)
        {
            _objectContainer = objectContainer;
            _objectContext = objectContext;

            var container = objectContainer.Resolve<IContainer>();

            _controller = new TransferConnectionInvitationsController(_objectContainer.Resolve<IMapper>(),
                _objectContainer.Resolve<IMediator>());

            _dasLevyRepository = container.GetInstance<IDasLevyRepository>();

            container.Inject(_controller);
        }

        [Given(@"account ([^ ]*) is a sender to account ([^ ]*)")]
        public void GivenAccountBisASenderToAccountC(string senderAccountName, string existingReceiverAccountName)
        {
            var senderAccount = _objectContext.Accounts[senderAccountName];
            var existingReceiverAccount = _objectContext.Accounts[existingReceiverAccountName];

            senderAccount.SentTransferConnectionInvitations.Add(
                new TransferConnectionInvitation(senderAccount, existingReceiverAccount, new User()));

            _objectContext.Accounts[senderAccountName] = senderAccount;
        }
        

        [Then(@"we are notified that the connection has been sent")]
        public void ThenWeAreNotifiedThatTheConnectionHasBeenSent()
        {
            Assert.IsInstanceOf<RedirectToRouteResult>(_actionResult);

            Assert.AreEqual("Sent", ((RedirectToRouteResult)_actionResult).RouteValues["Action"]);

            Assert.IsTrue(int.Parse(((RedirectToRouteResult)_actionResult).RouteValues["transferConnectionInvitationId"].ToString()) > 0);
        }

        [Then(@"we are notified that there was a failure when user ([^ ]*) of account ([^ ]*) sends a transfer connection invitation to account ([^ ]*)")]
        public async Task ThenWeAreNotifiedThatThereWasAFailureWhenUserDaveOfAccountASendsATransferConnectionInvitationToAccountB(string userName, string senderAccountName, string receiverAccountName)
        {
            try
            {
                await WhenAccountASendsATransferConnectionInvitationToAccountB(userName, senderAccountName,
                    receiverAccountName);

                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception)
            {
                Assert.Pass("Expected exception was thrown");
            }
        }

        [When(@"user ([^ ]*) of account ([^ ]*) sends a transfer connection invitation to account ([^ ]*)")]
        public async Task WhenAccountASendsATransferConnectionInvitationToAccountB(string userName, string senderAccountName, string receiverAccountName)
        {
            var senderAccount = _objectContext.Accounts[senderAccountName];
            var receiverAccount = _objectContext.Accounts[receiverAccountName];
            var user = _objectContext.Users[userName];

            var viewModel = new SendTransferConnectionInvitationViewModel
            {
                Choice = "Confirm",
                 SendTransferConnectionInvitationCommand  = new SendTransferConnectionInvitationCommand
                 {
                    ReceiverAccountPublicHashedId = receiverAccount.PublicHashedId,
                     AccountId = senderAccount.Id,
                     UserId = user.Id
                 }
            };

            _actionResult = await _controller.Send(viewModel);
        }

        //[Given(@"account ([^ ]*) has a signed agreement enabling transfers")]
        //public void GivenAccountHasSignedTransferAgreement(string accountName)
        //{
        //    var account = _objectContext.Accounts[accountName];

        //    _objectContext.Accounts[accountName] = account;
        //}

        //[Then(@"we are notified that there was a failure when the connection is sent")]
        //public void ThenWeAreNotifiedThatThereWasAFailureWhenTheConnectionIsSent()
        //{
        //    ScenarioContext.Current.Pending();
        //}


        //[Then(@"the sender is informed the receiver is already a sender")]
        //public void ThenSenderInformedReceiverAlreadyASender()
        //{
        //    Assert.IsInstanceOf<ViewResult>(_actionResult);

        //    Assert.AreEqual("Send", ((ViewResult)_actionResult).ViewName);
        //    Assert.AreEqual("Send", ((SendTransferConnectionInvitationViewModel)((ViewResult)_actionResult).Model).Choice);
        //}

    }
}
