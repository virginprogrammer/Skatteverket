using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var request = context.Request;
        var correlationId = context.Items["CorrelationId"]?.ToString();

        _logger.LogInformation(
            "Request started: {Method} {Path} {QueryString} [CorrelationId: {CorrelationId}]",
            request.Method,
            request.Path,
            request.QueryString,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Request completed: {Method} {Path} {StatusCode} in {ElapsedMilliseconds}ms [CorrelationId: {CorrelationId}]",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}