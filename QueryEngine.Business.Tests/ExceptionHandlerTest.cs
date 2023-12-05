using APIVersioning_Swagger.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QueryEngine.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QueryEngine.Business.Tests
{
    public class ExceptionHandlerTest
    {
        [Fact]
        public async void InvokeAsync_ApiBadRequestException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var next = new RequestDelegate(_ => throw new BadRequestException("Resource not found."));

            var middleware = new ExceptionMiddleWare();

            // Act
            await middleware.InvokeAsync(context,next);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest,context.Response.StatusCode);

        }

        [Fact]
        public async void InvokeAsync_InternalServerException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var next = new RequestDelegate(_ => throw new Exception("Resource not found."));

            var middleware = new ExceptionMiddleWare();

            // Act
            await middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

        }
    }
}
