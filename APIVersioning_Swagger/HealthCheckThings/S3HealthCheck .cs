using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace APIVersioning_Swagger.HealthCheckThings
{
    public class S3HealthCheck : IHealthCheck
    {
        private readonly IAmazonS3 _s3Client;

        public S3HealthCheck(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Replace with your S3 bucket name
                var bucketName = "query-engine-test-bucket";
                var listObjectsResponse = await _s3Client.ListObjectsAsync(new ListObjectsRequest
                {
                    BucketName = bucketName,
                    MaxKeys = 1
                }, cancellationToken);

                if (listObjectsResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Unhealthy("S3 bucket is not accessible.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"S3 bucket check failed: {ex.Message}");
            }
        }
    }
}
