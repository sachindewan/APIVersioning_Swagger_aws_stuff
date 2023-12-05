using Serilog.Context;
using Serilog.Enrichers;

namespace APIVersioning_Swagger.Middleware
{
    public class CorrelationIdMiddleware : IMiddleware
    {
        private const string CorrelationIdEnriche = "CorrelationIdEnricher+CorrelationId";
        private static readonly string CorrelationIdItemName = typeof(CorrelationIdEnricher).Name + "+CorrelationId";
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationId = context.Items[CorrelationIdEnriche]?.ToString();
            if (correlationId == null)
            {
                correlationId = Guid.NewGuid().ToString();
            }
            context.TraceIdentifier = correlationId;
            context.Items[CorrelationIdItemName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(context);
            }
        }
    }
}
