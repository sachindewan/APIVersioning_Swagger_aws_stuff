namespace APIVersioning_Swagger.Services
{
    public class HttpContextWrapper : ICorrelationIdProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="contextAccessor">The http context accessor</param>
        public HttpContextWrapper(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor), "Ensure you have bound IHttpContextAccessor into your service collection");
        }

        /// <inheritdoc />
        public string GetCorrelationId()
        {
            return _contextAccessor?.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        }
    }
}
