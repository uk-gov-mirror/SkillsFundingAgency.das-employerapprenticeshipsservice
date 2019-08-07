using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.MarkerInterfaces;
using SFA.DAS.EmployerAccounts.Models.TransferConnections;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Queries.SendTransferConnectionInvitation
{
    public class SendTransferConnectionInvitationQueryHandler : IAsyncRequestHandler<SendTransferConnectionInvitationQuery, SendTransferConnectionInvitationResponse>
    {
        private readonly Lazy<EmployerAccountsDbContext> _db;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IPublicHashingService _publicHashingService;

        public SendTransferConnectionInvitationQueryHandler(
            Lazy<EmployerAccountsDbContext> db,
            IConfigurationProvider configurationProvider,
            IPublicHashingService publicHashingService)
        {
            _db = db;
            _configurationProvider = configurationProvider;
            _publicHashingService = publicHashingService;
        }

        public async Task<SendTransferConnectionInvitationResponse> Handle(SendTransferConnectionInvitationQuery message)
        {
            var receiverAccount = _publicHashingService.TryDecodeValue(message.ReceiverAccountPublicHashedId, out var receiverAccountId)
                ? await _db.Value.Accounts.ProjectTo<AccountDto>(_configurationProvider).SingleOrDefaultAsync(a => a.Id == receiverAccountId)
                : null;

            if (receiverAccount == null || receiverAccount.Id == message.AccountId)
            {
                var ve = new ValidationException();
                ve.AddError<SendTransferConnectionInvitationQuery>(q => q.ReceiverAccountPublicHashedId, "You must enter a valid account ID");
                throw ve;
            }

            var isReceiverASender = await _db.Value.TransferConnectionInvitations.AnyAsync(i =>
                i.SenderAccountId == receiverAccount.Id && (
                i.Status == TransferConnectionInvitationStatus.Pending ||
                i.Status == TransferConnectionInvitationStatus.Approved));

            if (isReceiverASender)
            {
                var ve = new ValidationException();
                ve.AddError<SendTransferConnectionInvitationQuery>(q => q.ReceiverAccountPublicHashedId, "You can't connect with this employer because they already have pending or accepted connection requests");
                throw ve;
            }

            var anyTransferConnectionInvitations = await _db.Value.TransferConnectionInvitations.AnyAsync(i => (
                i.SenderAccount.Id == message.AccountId.Value && i.ReceiverAccount.Id == receiverAccount.Id ||
                i.SenderAccount.Id == receiverAccount.Id && i.ReceiverAccount.Id == message.AccountId.Value) && (
                i.Status == TransferConnectionInvitationStatus.Pending ||
                i.Status == TransferConnectionInvitationStatus.Approved));

            if (anyTransferConnectionInvitations)
            {
                var ve = new ValidationException();
                ve.AddError<SendTransferConnectionInvitationQuery>(q => q.ReceiverAccountPublicHashedId, "You can't connect with this employer because they already have a pending or accepted connection request");
                throw ve;
            }

            var senderAccount = await _db.Value.Accounts.ProjectTo<AccountDto>(_configurationProvider).SingleOrDefaultAsync(a => a.Id == message.AccountId);

            return new SendTransferConnectionInvitationResponse
            {
                ReceiverAccount = receiverAccount,
                SenderAccount = senderAccount,
            };
        }
    }
}