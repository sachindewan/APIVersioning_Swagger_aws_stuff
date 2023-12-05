using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace APIVersioning_Swagger.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _service;
        public HealthController(HealthCheckService service)
        {
            _service = service;
        }
        [HttpGet]
        [ProducesResponseType(204)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> Get()
        {
            var report = await _service.CheckHealthAsync();
            return report.Status == HealthStatus.Healthy ? NoContent() : StatusCode((int)HttpStatusCode.ServiceUnavailable, report);
        }
    }
}
