using MomsdeklarationAPI.Services;
using System.Text;

namespace MomsdeklarationAPI.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        if (!ShouldAudit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var operation = GetOperationName(context);
        var requestBody = await ReadRequestBodyAsync(context);
        var (redovisare, redovisningsperiod) = ExtractParametersFromPath(context.Request.Path);

        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            var result = context.Response.StatusCode < 400 ? "SUCCESS" : "FAILURE";

            await auditService.LogApiCallAsync(
                operation,
                redovisare ?? "N/A",
                redovisningsperiod,
                requestBody,
                result);

            if (IsDataAccessOperation(operation))
            {
                await auditService.LogDataAccessAsync(
                    "MomsDeklaration",
                    $"{redovisare}/{redovisningsperiod}",
                    GetDataAction(context.Request.Method),
                    context.User);
            }
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private bool ShouldAudit(PathString path)
    {
        var excludedPaths = new[] { "/health", "/ping", "/swagger", "/favicon.ico" };
        return !excludedPaths.Any(excluded => 
            path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private string GetOperationName(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method;

        if (path.Contains("/utkast"))
        {
            return method switch
            {
                "GET" => "GetDraft",
                "POST" => "CreateDraft",
                "DELETE" => "DeleteDraft",
                _ => "UtkastOperation"
            };
        }

        if (path.Contains("/kontrollera"))
            return "ValidateDraft";

        if (path.Contains("/las"))
        {
            return method switch
            {
                "PUT" => "LockDraft",
                "DELETE" => "UnlockDraft",
                _ => "LockOperation"
            };
        }

        if (path.Contains("/inlamnat"))
            return "GetSubmitted";

        if (path.Contains("/beslutat"))
            return "GetDecided";

        return "UnknownOperation";
    }

    private async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) && 
            !HttpMethods.IsPut(context.Request.Method))
        {
            return null;
        }

        context.Request.EnableBuffering();
        
        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        return string.IsNullOrEmpty(body) ? null : body;
    }

    private (string? redovisare, string? redovisningsperiod) ExtractParametersFromPath(PathString path)
    {
        var pathValue = path.Value;
        if (string.IsNullOrEmpty(pathValue))
            return (null, null);

        var segments = pathValue.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length >= 4)
        {
            return (segments[^2], segments[^1]);
        }

        return (null, null);
    }

    private bool IsDataAccessOperation(string operation)
    {
        var dataAccessOperations = new[] 
        { 
            "GetDraft", "CreateDraft", "DeleteDraft", 
            "GetSubmitted", "GetDecided", "ValidateDraft" 
        };
        
        return dataAccessOperations.Contains(operation);
    }

    private string GetDataAction(string httpMethod)
    {
        return httpMethod switch
        {
            "GET" => "READ",
            "POST" => "CREATE",
            "PUT" => "UPDATE",
            "DELETE" => "DELETE",
            _ => "UNKNOWN"
        };
    }
}