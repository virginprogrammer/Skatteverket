using Microsoft.Extensions.Caching.Memory;
using System.Net;
using MomsdeklarationAPI.Models.Responses;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var rateLimitKey = $"rate_limit_{clientId}";
        
        var requestCount = _cache.Get<int>(rateLimitKey);
        
        if (requestCount >= _options.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Requests: {RequestCount}/{MaxRequests}",
                clientId, requestCount, _options.MaxRequests);

            await HandleRateLimitExceeded(context);
            return;
        }

        _cache.Set(rateLimitKey, requestCount + 1, _options.TimeWindow);

        context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", 
            Math.Max(0, _options.MaxRequests - requestCount - 1).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", 
            DateTimeOffset.UtcNow.Add(_options.TimeWindow).ToUnixTimeSeconds().ToString());

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var userIdentity = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userIdentity))
        {
            return $"user_{userIdentity}";
        }

        var clientId = context.Request.Headers["Client-ID"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientId))
        {
            return $"client_{clientId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ipAddress))
        {
            return $"ip_{ipAddress}";
        }

        return "anonymous";
    }

    private bool IsExcludedPath(PathString path)
    {
        var excludedPaths = new[] { "/health", "/ping", "/swagger" };
        return excludedPaths.Any(excluded => path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private async Task HandleRateLimitExceeded(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Status = 429,
            Error = "Too Many Requests",
            Message = $"Rate limit exceeded. Maximum {_options.MaxRequests} requests per {_options.TimeWindow.TotalMinutes} minutes.",
            Path = context.Request.Path,
            CorrelationId = context.Items["CorrelationId"]?.ToString()
        };

        context.Response.Headers.Add("Retry-After", ((int)_options.TimeWindow.TotalSeconds).ToString());

        var json = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
        await context.Response.WriteAsync(json);
    }
}

public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(15);
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, RateLimitOptions options)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}