using System.Net;
using MomsdeklarationAPI.Models.Responses;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        _logger.LogError(exception, "Unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path,
            CorrelationId = context.Items["CorrelationId"]?.ToString()
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Unauthorized";
                errorResponse.Message = "Authentication failed or token expired";
                break;

            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Bad Request";
                errorResponse.Message = exception.Message;
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Not Found";
                errorResponse.Message = exception.Message;
                break;

            case HttpRequestException httpEx:
                response.StatusCode = (int)HttpStatusCode.BadGateway;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "External Service Error";
                errorResponse.Message = httpEx.Message;
                break;

            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Request Timeout";
                errorResponse.Message = "The request took too long to process";
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Conflict";
                errorResponse.Message = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Status = response.StatusCode;
                errorResponse.Error = "Internal Server Error";
                errorResponse.Message = "An unexpected error occurred";
                
                if (IsDevelopment(context))
                {
                    errorResponse.Details = new Dictionary<string, object>
                    {
                        ["exception"] = exception.GetType().Name,
                        ["message"] = exception.Message,
                        ["stackTrace"] = exception.StackTrace ?? string.Empty
                    };
                }
                break;
        }

        var jsonResponse = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
        await response.WriteAsync(jsonResponse);
    }

    private bool IsDevelopment(HttpContext context)
    {
        var environment = context.RequestServices.GetService<IWebHostEnvironment>();
        return environment?.IsDevelopment() ?? false;
    }
}