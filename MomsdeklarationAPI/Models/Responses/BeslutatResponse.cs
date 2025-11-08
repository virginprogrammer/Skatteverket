using MomsdeklarationAPI.Models.DTOs;
using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Responses;

public class BeslutatGetResponse
{
    [JsonProperty("momsuppgiftBeslut")]
    public MomsuppgiftBeslut MomsuppgiftBeslut { get; set; } = new();

    [JsonProperty("beslutsdatum")]
    public DateTime Beslutsdatum { get; set; }

    [JsonProperty("beslutstyp")]
    public string Beslutstyp { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

public class BeslutatPostResponse
{
    [JsonProperty("beslutade")]
    public List<BeslutatItem> Beslutade { get; set; } = new();
}

public class BeslutatItem
{
    [JsonProperty("redovisare")]
    public string Redovisare { get; set; } = string.Empty;

    [JsonProperty("redovisningsperiod")]
    public string Redovisningsperiod { get; set; } = string.Empty;

    [JsonProperty("momsuppgiftBeslut")]
    public MomsuppgiftBeslut MomsuppgiftBeslut { get; set; } = new();

    [JsonProperty("beslutsdatum")]
    public DateTime Beslutsdatum { get; set; }

    [JsonProperty("beslutstyp")]
    public string Beslutstyp { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}