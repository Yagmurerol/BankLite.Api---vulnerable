using System.Collections.Concurrent;
using System.Net;

namespace BankLite.Api.Middleware;

/// <summary>
/// Simple in-memory rate limiting middleware
/// Prevents brute force attacks on sensitive endpoints
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RequestCounter> _requests = new();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path.Value?.ToLower() ?? "";
        
        // ✅ SECURITY: Rate limit sensitive endpoints
        if (endpoint.Contains("/auth/login") || 
            endpoint.Contains("/auth/register") ||
            endpoint.Contains("/transfers/initiate") ||
            endpoint.Contains("/transfers/") && endpoint.Contains("/confirm"))
        {
            var clientId = GetClientIdentifier(context);
            
            if (!IsAllowed(clientId, endpoint))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Too many requests. Please try again later.",
                    retryAfter = 60 
                });
                return;
            }
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use IP address as identifier
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // If user is authenticated, use user ID for more accurate limiting
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        return userId ?? ip;
    }

    private bool IsAllowed(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";
        var now = DateTime.UtcNow;

        var counter = _requests.GetOrAdd(key, _ => new RequestCounter());

        lock (counter)
        {
            // Clean old requests (older than 1 minute)
            counter.Timestamps.RemoveAll(t => (now - t).TotalSeconds > 60);

            // ✅ SECURITY: Max 5 requests per minute per endpoint
            if (counter.Timestamps.Count >= 5)
            {
                return false;
            }

            counter.Timestamps.Add(now);
            return true;
        }
    }

    private class RequestCounter
    {
        public List<DateTime> Timestamps { get; } = new();
    }
}
