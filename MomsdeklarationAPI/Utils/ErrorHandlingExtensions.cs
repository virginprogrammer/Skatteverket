using MomsdeklarationAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MomsdeklarationAPI.Utils;

public static class ErrorHandlingExtensions
{
    public static IActionResult CreateErrorResponse(this ControllerBase controller, 
        HttpStatusCode statusCode, 
        string message, 
        Exception? exception = null,
        Dictionary<string, object>? additionalDetails = null)
    {
        var errorResponse = new ErrorResponse
        {
            Status = (int)statusCode,
            Error = GetErrorTitle(statusCode),
            Message = message,
            Path = controller.Request.Path,
            Timestamp = DateTime.UtcNow,
            CorrelationId = controller.HttpContext.Items["CorrelationId"]?.ToString()
        };

        if (controller.HttpContext.RequestServices
            .GetService<IWebHostEnvironment>()?
            .IsDevelopment() == true && exception != null)
        {
            errorResponse.Details = new Dictionary<string, object>
            {
                ["exception"] = exception.GetType().Name,
                ["stackTrace"] = exception.StackTrace ?? string.Empty,
                ["innerException"] = exception.InnerException?.Message ?? string.Empty
            };

            if (additionalDetails != null)
            {
                foreach (var detail in additionalDetails)
                {
                    errorResponse.Details[detail.Key] = detail.Value;
                }
            }
        }

        return controller.StatusCode((int)statusCode, errorResponse);
    }

    public static IActionResult CreateValidationErrorResponse(this ControllerBase controller,
        string message,
        Dictionary<string, string[]>? validationErrors = null)
    {
        var errorResponse = new ErrorResponse
        {
            Status = 400,
            Error = "Validation Failed",
            Message = message,
            Path = controller.Request.Path,
            Timestamp = DateTime.UtcNow,
            CorrelationId = controller.HttpContext.Items["CorrelationId"]?.ToString()
        };

        if (validationErrors?.Any() == true)
        {
            errorResponse.Details = new Dictionary<string, object>
            {
                ["validationErrors"] = validationErrors
            };
        }

        return controller.BadRequest(errorResponse);
    }

    private static string GetErrorTitle(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
            HttpStatusCode.NotAcceptable => "Not Acceptable",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.Gone => "Gone",
            HttpStatusCode.UnsupportedMediaType => "Unsupported Media Type",
            HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
            HttpStatusCode.TooManyRequests => "Too Many Requests",
            HttpStatusCode.InternalServerError => "Internal Server Error",
            HttpStatusCode.NotImplemented => "Not Implemented",
            HttpStatusCode.BadGateway => "Bad Gateway",
            HttpStatusCode.ServiceUnavailable => "Service Unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway Timeout",
            _ => "Error"
        };
    }

    public static async Task<T?> ExecuteWithErrorHandlingAsync<T>(
        Func<Task<T>> operation,
        ILogger logger,
        string operationName) where T : class
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex)
        {
            logger.Error(ex, "HTTP request failed in {OperationName}", operationName);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            logger.Error(ex, "Operation timed out in {OperationName}", operationName);
            throw new TimeoutException($"Operation {operationName} timed out", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex, "Unauthorized access in {OperationName}", operationName);
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error in {OperationName}", operationName);
            throw;
        }
    }

    public static async Task ExecuteWithErrorHandlingAsync(
        Func<Task> operation,
        ILogger logger,
        string operationName)
    {
        try
        {
            await operation();
        }
        catch (HttpRequestException ex)
        {
            logger.Error(ex, "HTTP request failed in {OperationName}", operationName);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            logger.Error(ex, "Operation timed out in {OperationName}", operationName);
            throw new TimeoutException($"Operation {operationName} timed out", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex, "Unauthorized access in {OperationName}", operationName);
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error in {OperationName}", operationName);
            throw;
        }
    }
}

public static class HttpContextExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
    }

    public static string GetClientIpAddress(this HttpContext context)
    {
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

    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }
}