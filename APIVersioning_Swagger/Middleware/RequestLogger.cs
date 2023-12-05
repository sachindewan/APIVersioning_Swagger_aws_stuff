using Azure.Core;
using Serilog.Context;
using Serilog.Enrichers;
using System.Text;

namespace APIVersioning_Swagger.Middleware
{
    public class RequestLogger : IMiddleware
    {
        private readonly ILogger<RequestLogger> _logger;

        public RequestLogger(ILogger<RequestLogger> logger)
        {
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
        var CorrelationIdItemName = typeof(CorrelationIdEnricher).Name + "+CorrelationId";

        var item = context.Items[CorrelationIdItemName];

            try
            {
                _logger.LogInformation(
                    "Starting Request {RequestMethod} {TraceId} {Path} {StatusCode}",
                    context.Request?.Method,
                    context.TraceIdentifier,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
                // Capture the request body
               // if (context.Request.ContentLength.HasValue && context.Request.ContentLength > 0)
               // {
               //     context.Request.EnableBuffering();

               //     using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
               //     {
               //         string body = await reader.ReadToEndAsync();
               //         _logger.LogInformation("Request Body: {Body}", body);

               //         // Reset the stream so the request can be processed further
               //         context.Request.Body.Position = 0;
               //     }
               // }
               //var request = ExtractRequestBody(context);
                await next(context);
            }
            finally
            {
                _logger.LogInformation(
                    "Request Complete {RequestMethod} {TraceId} {Path} {StatusCode}",
                    context.Request?.Method,
                     context.TraceIdentifier,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
            }
        }
        private static string ExtractRequestBody(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            Stream body = httpContext.Request.Body;
            byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];

            httpContext.Request.Body.Read(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = body;

            return requestBody;
        }
    }

}
