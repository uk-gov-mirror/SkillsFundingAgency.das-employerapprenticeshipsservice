namespace SFA.DAS.EmployerAccounts.Api.Orchestrators
{
    using System.Threading.Tasks;

    using AutoMapper;

    using MediatR;

    public class AgreementOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AgreementOrchestrator(IMediator mediator, IMapper mapper)
        {
            this._mediator = mediator;
            this._mapper = mapper;
        }
        
        public async Task<OrchestratorResponse<EmployerAgreementView>> GetAgreement(string hashedAgreementId)
        {
            var response = await this._mediator.SendAsync(new GetEmployerAgreementByIdRequest
            {
                HashedAgreementId = hashedAgreementId
            });

            return new OrchestratorResponse<EmployerAgreementView>
            {
                Data = this._mapper.Map<EmployerAgreementView>(response.EmployerAgreement)
            };
        }
    }
}