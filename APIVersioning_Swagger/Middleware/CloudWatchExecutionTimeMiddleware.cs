using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace APIVersioning_Swagger.Middleware
{
    public class CloudWatchExecutionTimeMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IAmazonCloudWatch _amazonCloudWatch;

        public CloudWatchExecutionTimeMiddleware(ILogger<CloudWatchExecutionTimeMiddleware> logger, IAmazonCloudWatch amazonCloudWatch)
        {
            _logger = logger;
            _amazonCloudWatch = amazonCloudWatch;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            await next(context);
            stopWatch.Stop();

            try
            {
                await _amazonCloudWatch.PutMetricDataAsync(new PutMetricDataRequest
                {
                    Namespace = "Demo",
                    MetricData = new List<MetricDatum>
                    {
                        new MetricDatum
                        {
                            MetricName = "AspNetExecutionTime",
                            Value = stopWatch.ElapsedMilliseconds,
                            Unit = StandardUnit.Milliseconds,
                            TimestampUtc = DateTime.UtcNow,
                            Dimensions = new List<Dimension>
                            {
                                new Dimension
                                {
                                    Name = "Method",
                                    Value = context.Request.Method
                                },
                                new Dimension
                                {
                                    Name = "Path",
                                    Value = context.Request.Path
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
               
                _logger.LogCritical(ex, "Failed to send CloudWatch Metric");
            }
        }
    }
}
