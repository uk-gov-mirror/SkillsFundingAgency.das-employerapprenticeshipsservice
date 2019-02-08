namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using MediatR;

    using SFA.DAS.EmployerAccounts.Api.Attributes;
    using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities;

    [ApiAuthorize(Roles = "ReadUserAccounts")]
    [RoutePrefix("api/accountlegalentities")]
    public class AccountLegalEntitiesController : ApiController
    {
        private readonly IMediator _mediator;

        public AccountLegalEntitiesController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [Route]
        public async Task<IHttpActionResult> Get([FromUri] GetAccountLegalEntitiesRequest query)
        {
            var response = await this._mediator.SendAsync(query);
            return Ok(response.Entities);
        }
    }
}
