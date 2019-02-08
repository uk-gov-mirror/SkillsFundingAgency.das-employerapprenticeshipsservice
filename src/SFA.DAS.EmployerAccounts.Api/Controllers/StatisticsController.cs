namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using MediatR;

    [ApiAuthorize(Roles = "ReadUserAccounts")]
    [RoutePrefix("api/statistics")]
    public class StatisticsController : ApiController
    {
        private readonly IMediator _mediator;

        public StatisticsController(IMediator mediator)
        {
            this._mediator = mediator;
        }
        
        [Route("")]
        public async Task<IHttpActionResult> GetStatistics()
        {
            var response = await this._mediator.SendAsync(new GetStatisticsQuery());
            return Ok(response.Statistics);
        }
    }
}
