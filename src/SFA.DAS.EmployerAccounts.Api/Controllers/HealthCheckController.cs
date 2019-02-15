namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Web.Http;

    public class HealthCheckController : ApiController
    {
        [Route("api/HealthCheck")]
        public IHttpActionResult GetStatus()
        {
            return this.Ok();
        }
    }
}