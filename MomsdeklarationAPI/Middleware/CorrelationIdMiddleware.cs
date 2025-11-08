using Serilog.Context;

namespace MomsdeklarationAPI.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CORRELATION_ID_HEADER = "X-Correlation-Id";
    private const string SKV_CORRELATION_ID_HEADER = "skv_correlation_id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add(CORRELATION_ID_HEADER, correlationId);
        context.Response.Headers.Add(SKV_CORRELATION_ID_HEADER, correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private string GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CORRELATION_ID_HEADER, out var correlationId) && 
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        if (context.Request.Headers.TryGetValue(SKV_CORRELATION_ID_HEADER, out var skvCorrelationId) && 
            !string.IsNullOrWhiteSpace(skvCorrelationId))
        {
            return skvCorrelationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}