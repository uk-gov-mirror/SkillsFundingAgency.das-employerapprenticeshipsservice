namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using MediatR;

    [ApiAuthorize(Roles = "ReadUserAccounts")]
    [RoutePrefix("api/accounts/{hashedAccountId}/transfers/connections")]
    public class TransferConnectionsController : ApiController
    {
        private readonly IMediator _mediator;

        public TransferConnectionsController(IMediator mediator)
        {
            this._mediator = mediator;
        }
        
        [Route]
        public async Task<IHttpActionResult> GetTransferConnections([FromUri] GetTransferConnectionsQuery query)
        {
            var response = await this._mediator.SendAsync(query);
            return Ok(response.TransferConnections);
        }
    }
}