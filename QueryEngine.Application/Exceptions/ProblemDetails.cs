using System.Net;

namespace QueryEngine.Application.Exceptions
{
    public class ProblemDetails 
    {
        public string Title { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
        public string TraceId { get; set; }
    }
}
