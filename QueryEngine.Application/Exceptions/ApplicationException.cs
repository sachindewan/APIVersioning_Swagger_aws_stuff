using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace QueryEngine.Application.Exceptions
{
    public class ApplicationException : Exception,IServiceException
    {
        public ApplicationException(string message):base(message) {
            ErrorDetails =    new ProblemDetails()
            {
                Title = "Request validation error",
                Status = HttpStatusCode.BadRequest,
                Type = "https://httpstatuses.com/500",
                Detail = message,
            };
        }

        public ProblemDetails ErrorDetails { get; set; }
    }
    
}
