using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.DTOs;

public class MomsuppgiftBeslut
{
    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("beslutsdatum")]
    public DateTime? Beslutsdatum { get; set; }

    [JsonProperty("beslutstyp")]
    public string? Beslutstyp { get; set; }

    [JsonProperty("beslutsreferens")]
    public string? Beslutsreferens { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("kommentar")]
    public string? Kommentar { get; set; }
}