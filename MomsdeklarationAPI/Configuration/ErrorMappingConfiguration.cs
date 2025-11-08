using System.Net;

namespace MomsdeklarationAPI.Configuration;

public static class ErrorMappingConfiguration
{
    public static Dictionary<string, SkatteverketErrorMapping> GetErrorMappings()
    {
        return new Dictionary<string, SkatteverketErrorMapping>
        {
            // Authentication errors
            ["AUTH001"] = new("Invalid client credentials", HttpStatusCode.Unauthorized),
            ["AUTH002"] = new("Token expired", HttpStatusCode.Unauthorized),
            ["AUTH003"] = new("Invalid token", HttpStatusCode.Unauthorized),
            ["AUTH004"] = new("Certificate validation failed", HttpStatusCode.Forbidden),
            ["AUTH005"] = new("Insufficient permissions", HttpStatusCode.Forbidden),

            // Validation errors
            ["VAL001"] = new("Invalid organization number", HttpStatusCode.BadRequest),
            ["VAL002"] = new("Invalid reporting period", HttpStatusCode.BadRequest),
            ["VAL003"] = new("Missing required field", HttpStatusCode.BadRequest),
            ["VAL004"] = new("Invalid field value", HttpStatusCode.BadRequest),
            ["VAL005"] = new("Data validation failed", HttpStatusCode.UnprocessableEntity),
            ["VAL006"] = new("Business rule validation failed", HttpStatusCode.UnprocessableEntity),

            // Resource errors
            ["RES001"] = new("Draft not found", HttpStatusCode.NotFound),
            ["RES002"] = new("Submitted declaration not found", HttpStatusCode.NotFound),
            ["RES003"] = new("Decided declaration not found", HttpStatusCode.NotFound),
            ["RES004"] = new("Reporting period not available", HttpStatusCode.NotFound),
            ["RES005"] = new("Organization not found", HttpStatusCode.NotFound),

            // State errors
            ["STATE001"] = new("Draft already exists", HttpStatusCode.Conflict),
            ["STATE002"] = new("Draft is locked", HttpStatusCode.Conflict),
            ["STATE003"] = new("Draft cannot be modified", HttpStatusCode.Conflict),
            ["STATE004"] = new("Invalid state transition", HttpStatusCode.Conflict),
            ["STATE005"] = new("Declaration already submitted", HttpStatusCode.Conflict),

            // Rate limiting
            ["RATE001"] = new("Rate limit exceeded", HttpStatusCode.TooManyRequests),
            ["RATE002"] = new("Quota exceeded", HttpStatusCode.TooManyRequests),

            // Server errors
            ["SYS001"] = new("Service temporarily unavailable", HttpStatusCode.ServiceUnavailable),
            ["SYS002"] = new("Internal processing error", HttpStatusCode.InternalServerError),
            ["SYS003"] = new("Database connection error", HttpStatusCode.ServiceUnavailable),
            ["SYS004"] = new("External service error", HttpStatusCode.BadGateway),
            ["SYS005"] = new("Timeout error", HttpStatusCode.RequestTimeout)
        };
    }

    public static SkatteverketErrorMapping MapHttpStatusToError(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => new("Bad request", statusCode),
            HttpStatusCode.Unauthorized => new("Authentication failed", statusCode),
            HttpStatusCode.Forbidden => new("Access denied", statusCode),
            HttpStatusCode.NotFound => new("Resource not found", statusCode),
            HttpStatusCode.MethodNotAllowed => new("Method not allowed", statusCode),
            HttpStatusCode.NotAcceptable => new("Not acceptable", statusCode),
            HttpStatusCode.Conflict => new("Conflict", statusCode),
            HttpStatusCode.Gone => new("Resource no longer available", statusCode),
            HttpStatusCode.UnsupportedMediaType => new("Unsupported media type", statusCode),
            HttpStatusCode.UnprocessableEntity => new("Unprocessable entity", statusCode),
            HttpStatusCode.TooManyRequests => new("Rate limit exceeded", statusCode),
            HttpStatusCode.InternalServerError => new("Internal server error", statusCode),
            HttpStatusCode.NotImplemented => new("Not implemented", statusCode),
            HttpStatusCode.BadGateway => new("Bad gateway", statusCode),
            HttpStatusCode.ServiceUnavailable => new("Service unavailable", statusCode),
            HttpStatusCode.GatewayTimeout => new("Gateway timeout", statusCode),
            _ => new("Unknown error", statusCode)
        };
    }

    public static string GetUserFriendlyMessage(string errorCode, HttpStatusCode statusCode)
    {
        var mappings = GetErrorMappings();
        
        if (mappings.TryGetValue(errorCode, out var mapping))
        {
            return mapping.UserMessage;
        }

        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Det finns problem med din förfrågan. Kontrollera att all data är korrekt.",
            HttpStatusCode.Unauthorized => "Du har inte behörighet att komma åt denna resurs. Kontrollera dina inloggningsuppgifter.",
            HttpStatusCode.Forbidden => "Du har inte tillräcklig behörighet för att utföra denna åtgärd.",
            HttpStatusCode.NotFound => "Den begärda resursen kunde inte hittas.",
            HttpStatusCode.Conflict => "Det finns en konflikt med den aktuella statusen för resursen.",
            HttpStatusCode.UnprocessableEntity => "Data kunde inte bearbetas på grund av valideringsfel.",
            HttpStatusCode.TooManyRequests => "För många förfrågningar. Vänta en stund innan du försöker igen.",
            HttpStatusCode.InternalServerError => "Ett internt fel har inträffat. Kontakta support om problemet kvarstår.",
            HttpStatusCode.ServiceUnavailable => "Tjänsten är tillfälligt otillgänglig. Försök igen senare.",
            _ => "Ett oväntat fel har inträffat."
        };
    }
}

public record SkatteverketErrorMapping(string UserMessage, HttpStatusCode StatusCode)
{
    public bool IsRetryable => StatusCode == HttpStatusCode.ServiceUnavailable ||
                              StatusCode == HttpStatusCode.RequestTimeout ||
                              StatusCode == HttpStatusCode.TooManyRequests ||
                              StatusCode == HttpStatusCode.BadGateway ||
                              StatusCode == HttpStatusCode.GatewayTimeout;
}