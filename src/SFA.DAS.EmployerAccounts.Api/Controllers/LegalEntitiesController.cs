namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using MediatR;

    using SFA.DAS.Validation.WebApi;

    [RoutePrefix("api/accounts/{hashedAccountId}/legalentities")]
    public class LegalEntitiesController : ApiController
    {
        private readonly AccountsOrchestrator _orchestrator;
        private readonly IMediator _mediator;

        public LegalEntitiesController(AccountsOrchestrator orchestrator, IMediator mediator)
        {
            this._orchestrator = orchestrator;
            this._mediator = mediator;
        }

        [Route("", Name = "GetLegalEntities")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetLegalEntities(string hashedAccountId)
        {
            var result = await this._orchestrator.GetAccount(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }
            
            result.Data.LegalEntities.ForEach(l => l.Href = this.Url.Route("GetLegalEntity", new { hashedAccountId, legalEntityId = l.Id }));

            return Ok(result.Data.LegalEntities);
        }

        [Route("{legalEntityId}", Name = "GetLegalEntity")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpNotFoundForNullModel]
        public async Task<IHttpActionResult> GetLegalEntity([FromUri] GetLegalEntityQuery query)
        {
            var response = await this._mediator.SendAsync(query);
            return Ok(response.LegalEntity);
        }
    }
}