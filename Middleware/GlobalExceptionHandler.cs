using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace BankLite.Api.Middleware;

/// <summary>
/// Global exception handler that prevents sensitive information disclosure
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the full exception details securely
        _logger.LogError(exception, 
            "An unhandled exception occurred. Path: {Path}, Method: {Method}", 
            httpContext.Request.Path, 
            httpContext.Request.Method);

        var response = httpContext.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // ✅ SECURITY: Never expose stack traces, table names, or sensitive details in production
        var errorResponse = new
        {
            error = "An error occurred while processing your request.",
            statusCode = response.StatusCode,
            // Only show detailed error in development
            details = _env.IsDevelopment() ? exception.Message : null
        };

        await response.WriteAsync(
            JsonSerializer.Serialize(errorResponse),
            cancellationToken);

        return true;
    }
}
