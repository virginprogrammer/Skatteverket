using MomsdeklarationAPI.Models.DTOs;
using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Responses;

public class UtkastResponse
{
    [JsonProperty("sparad")]
    public bool Sparad { get; set; }

    [JsonProperty("last")]
    public bool Last { get; set; }

    [JsonProperty("kontrollResultat")]
    public KontrollResultat KontrollResultat { get; set; } = new();

    [JsonProperty("signeringsLank")]
    public string? SigneringsLank { get; set; }
}

public class UtkastGetResponse
{
    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("kontrollResultat")]
    public KontrollResultat KontrollResultat { get; set; } = new();

    [JsonProperty("last")]
    public bool Last { get; set; }

    [JsonProperty("kommentar")]
    public string? Kommentar { get; set; }
}

public class UtkastPostMultiResponse
{
    [JsonProperty("utkast")]
    public List<UtkastItem> Utkast { get; set; } = new();
}

public class UtkastItem
{
    [JsonProperty("redovisare")]
    public string Redovisare { get; set; } = string.Empty;

    [JsonProperty("redovisningsperiod")]
    public string Redovisningsperiod { get; set; } = string.Empty;

    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("kontrollResultat")]
    public KontrollResultat KontrollResultat { get; set; } = new();

    [JsonProperty("last")]
    public bool Last { get; set; }

    [JsonProperty("kommentar")]
    public string? Kommentar { get; set; }
}