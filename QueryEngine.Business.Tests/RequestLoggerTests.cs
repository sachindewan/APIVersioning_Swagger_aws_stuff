using APIVersioning_Swagger.Authentication;
using APIVersioning_Swagger.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace QueryEngine.Business.Tests
{
    public class RequestLoggerTests
    {
        [Fact]
        public async Task InvokeAsync_LogsRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/api/some-endpoint";
            
            var logger = new TestLogger<RequestLogger>();
            //loggerMock.Setup(x => x.LogInformation("Starting Request {RequestMethod} {TraceId} {Path} {StatusCode}", "GET", context.TraceIdentifier, "/api/some-endpoint", 200));
            var next = new RequestDelegate((innerContext) => Task.CompletedTask);

            var middleware = new RequestLogger(logger); 

            // Act
            await middleware.InvokeAsync(context, next);
            // Assert
            //loggerMock.Verify(
            //    x => x.LogInformation("Starting Request {RequestMethod} {TraceId} {Path} {StatusCode}", "GET", context.TraceIdentifier, "/api/some-endpoint", 200),
            //    Times.Once);
            // Assert
            Assert.Contains($"Starting Request GET {context.TraceIdentifier} /api/some-endpoint 200", logger.LogMessages);
            //loggerMock.Verify(
            //                 x => x.LogInformation("Request Body: {Body}", It.IsAny<string>()),
            //                    Times.Never);

        }


        [Fact]
        public async Task InvokeAsync_CorrelatiId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/api/some-endpoint";

            var logger = new TestLogger<RequestLogger>();
            //loggerMock.Setup(x => x.LogInformation("Starting Request {RequestMethod} {TraceId} {Path} {StatusCode}", "GET", context.TraceIdentifier, "/api/some-endpoint", 200));
            var next = new RequestDelegate((innerContext) => Task.CompletedTask);

            var middleware = new CorrelationIdMiddleware();

            // Act
            await middleware.InvokeAsync(context, next);
            // Assert
            //loggerMock.Verify(
            //    x => x.LogInformation("Starting Request {RequestMethod} {TraceId} {Path} {StatusCode}", "GET", context.TraceIdentifier, "/api/some-endpoint", 200),
            //    Times.Once);
            // Assert
            Assert.Contains($"Starting Request GET {context.TraceIdentifier} /api/some-endpoint 200", logger.LogMessages);
            //loggerMock.Verify(
            //                 x => x.LogInformation("Request Body: {Body}", It.IsAny<string>()),
            //                    Times.Never);

        }
    }
    public class TestLogger<T> : ILogger<T>
    {
        private readonly List<string> _logMessages = new List<string>();

        public IList<string> LogMessages => _logMessages;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // You can customize this as needed based on the log level you want to test.
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            _logMessages.Add(message);
        }
    }
}
