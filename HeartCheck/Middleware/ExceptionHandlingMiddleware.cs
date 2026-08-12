using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace HeartCheck.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            var statusCode = exception switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var showDetails = !_environment.IsProduction() || statusCode != StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Type = $"https://httpstatuses.com/{statusCode}",
                Detail = showDetails
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };

            if (!_environment.IsProduction())
            {
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }

        private static string GetTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status404NotFound => "Not Found",
                _ => "Internal Server Error"
            };
        }
    }
}
