using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Builder;
using Shared.Exceptions;
using Shared.Models;
using System.Net;
using System.Text.Json;

namespace Shared.Middlewares
{
    public class GlobalExceptionMiddleware : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "❌ An error occurred: {Message}", exception.Message);

            object response;
            int statusCode;
            if (exception is BaseDomainException domainEx)
            {
                statusCode = (int)domainEx.StatusCode;
                var errors = new List<ApiError> { new ApiError { Code = domainEx.ErrorCode, Message = domainEx.Message } };

                response = statusCode switch
                {
                    400 => ResponseBuilder.BadRequest("Yêu cầu không hợp lệ ", errors),
                    401 => ResponseBuilder.Unauthorized(domainEx.Message),
                    403 => ResponseBuilder.Forbidden(domainEx.Message),
                    404 => ResponseBuilder.NotFound(domainEx.Message),
                    409 => ResponseBuilder.Conflict(domainEx.Message, errors),
                    429 => ResponseBuilder.Error("Quá nhiều yêu cầu. Vui lòng thử lại sau.", errors),
                    _ => ResponseBuilder.Error(domainEx.Message)
                };
            }

            else
            {
                // Lỗi không mong đợi
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = ResponseBuilder.InternalServerError(exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;
        }
    }
}
