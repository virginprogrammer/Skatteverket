using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.DTOs;

public class KontrollResultat
{
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("resultat")]
    public List<Kontroll> Resultat { get; set; } = new();
}

public class Kontroll
{
    [JsonProperty("kod")]
    public string Kod { get; set; } = string.Empty;

    [JsonProperty("typ")]
    public string Typ { get; set; } = string.Empty;

    [JsonProperty("meddelande")]
    public string Meddelande { get; set; } = string.Empty;

    [JsonProperty("falt")]
    public string? Falt { get; set; }
}