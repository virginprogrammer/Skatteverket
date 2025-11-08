using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Responses;

public class LasResponse
{
    [JsonProperty("last")]
    public bool Last { get; set; }

    [JsonProperty("lastTid")]
    public DateTime LastTid { get; set; }

    [JsonProperty("signeringsLank")]
    public string? SigneringsLank { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("meddelande")]
    public string? Meddelande { get; set; }
}