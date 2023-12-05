

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace APIVersioning_Swagger.HealthCheckThings
{
    public class ServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ServiceHealthCheck(IHttpClientFactory httpClientFactory,IHttpContextAccessor httpContextAccessor)
        {
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseAddress = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync("/WeatherForecast/HealthCheck");
                response.EnsureSuccessStatusCode();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Service is down: {ex.Message}");
            }
        }
    }
}
