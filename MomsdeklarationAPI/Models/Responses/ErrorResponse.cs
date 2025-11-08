using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Responses;

public class ErrorResponse
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; } = string.Empty;

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("path")]
    public string Path { get; set; } = string.Empty;

    [JsonProperty("correlationId")]
    public string? CorrelationId { get; set; }

    [JsonProperty("details")]
    public Dictionary<string, object>? Details { get; set; }
}