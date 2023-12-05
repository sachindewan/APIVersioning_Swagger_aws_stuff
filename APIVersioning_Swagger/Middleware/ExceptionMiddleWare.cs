using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QueryEngine.Application.Exceptions;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

namespace APIVersioning_Swagger.Middleware
{
    public class ExceptionMiddleWare : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }catch(Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                switch (ex)
                {

                    case IServiceException e:
                       
                        response.StatusCode = (int)e.ErrorDetails.Status;
                        e.ErrorDetails.TraceId = context.TraceIdentifier;
                        await response.WriteAsync(JsonConvert.SerializeObject(e.ErrorDetails));
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await response.WriteAsync(JsonConvert.SerializeObject(GetApplicationProblemDetails(ex, context.TraceIdentifier)));
                        break;

                }
            }

        }
        private QueryEngine.Application.Exceptions.ProblemDetails GetApplicationProblemDetails(Exception ex,string traceId)
        {
            return new QueryEngine.Application.Exceptions.ProblemDetails()
            {
                Title = "Internal Server Error",
                Status = HttpStatusCode.InternalServerError,
                Type = "https://httpstatuses.com/500",
                Detail = ex.Message.ToString(),
                TraceId = traceId,
            };
        }
    }
}
