using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace APIVersioning_Swagger.HealthCheckThings
{
    public class ParameterStoreHealthCheck : IHealthCheck
    {
        private const string parameterName = "/dev/ssm/secretkey";

        public ParameterStoreHealthCheck(IAmazonSimpleSystemsManagement ssmClient)
        {
            SsmClient = ssmClient;
        }

        public IAmazonSimpleSystemsManagement SsmClient { get; }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await SsmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = parameterName,
                }, cancellationToken);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Unhealthy("Parameter does not exist or cannot be accessed.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Parameter check failed: {ex.Message}");
            }
        }
    }
}
