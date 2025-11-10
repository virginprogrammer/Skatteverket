using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Services;

public interface IAuditService
{
    Task LogApiCallAsync(string operation, string redovisare, string? redovisningsperiod, object? requestData, string result);
    Task LogAuthenticationEventAsync(string eventType, ClaimsPrincipal? user, bool success, string? reason = null);
    Task LogDataAccessAsync(string dataType, string identifier, string action, ClaimsPrincipal? user);
    Task LogSecurityEventAsync(string eventType, string description, ClaimsPrincipal? user, string? ipAddress = null);
}

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ILogger<AuditService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogApiCallAsync(string operation, string redovisare, string? redovisningsperiod, object? requestData, string result)
    {
        var context = _httpContextAccessor.HttpContext;
        var user = context?.User?.Identity?.Name ?? "Anonymous";
        var correlationId = context?.Items["CorrelationId"]?.ToString();
        var ipAddress = GetClientIpAddress();

        _logger.LogInformation("API_CALL: Operation={Operation} User={User} Redovisare={Redovisare} Period={Period} Result={Result} IP={IP} CorrelationId={CorrelationId}",
            operation, user, redovisare, redovisningsperiod ?? "N/A", result, ipAddress, correlationId);

        await Task.CompletedTask;
    }

    public async Task LogAuthenticationEventAsync(string eventType, ClaimsPrincipal? user, bool success, string? reason = null)
    {
        var userName = user?.Identity?.Name ?? "Unknown";
        var ipAddress = GetClientIpAddress();

        if (success)
        {
            _logger.LogInformation("AUTH_SUCCESS: Event={EventType} User={User} IP={IP}",
                eventType, userName, ipAddress);
        }
        else
        {
            _logger.LogWarning("AUTH_FAILURE: Event={EventType} User={User} Reason={Reason} IP={IP}",
                eventType, userName, reason ?? "Unknown", ipAddress);
        }

        await Task.CompletedTask;
    }

    public async Task LogDataAccessAsync(string dataType, string identifier, string action, ClaimsPrincipal? user)
    {
        var userName = user?.Identity?.Name ?? "Unknown";
        var ipAddress = GetClientIpAddress();
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        _logger.LogInformation("DATA_ACCESS: Type={DataType} Identifier={Identifier} Action={Action} User={User} IP={IP} CorrelationId={CorrelationId}",
            dataType, identifier, action, userName, ipAddress, correlationId);

        await Task.CompletedTask;
    }

    public async Task LogSecurityEventAsync(string eventType, string description, ClaimsPrincipal? user, string? ipAddress = null)
    {
        var userName = user?.Identity?.Name ?? "Unknown";
        var clientIp = ipAddress ?? GetClientIpAddress();
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString();

        _logger.LogWarning("SECURITY_EVENT: Event={EventType} Description={Description} User={User} IP={IP} CorrelationId={CorrelationId}",
            eventType, description, userName, clientIp, correlationId);

        await Task.CompletedTask;
    }

    private string GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "Unknown";

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}