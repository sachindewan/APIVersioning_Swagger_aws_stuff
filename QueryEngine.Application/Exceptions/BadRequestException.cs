using System.Net;

namespace QueryEngine.Application.Exceptions
{
    public class BadRequestException : Exception, IServiceException
    {
        public BadRequestException(string message):base(message)
        {
            ErrorDetails = new ProblemDetails()
            {
                Title = "Request validation error",
                Status = HttpStatusCode.BadRequest,
                Type = "https://httpstatuses.com/400",
                Detail = message,
            };
        }
        public ProblemDetails ErrorDetails { get; set; }
    }
}
