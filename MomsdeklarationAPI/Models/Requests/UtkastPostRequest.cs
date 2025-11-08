using MomsdeklarationAPI.Models.DTOs;
using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Requests;

public class UtkastPostRequest
{
    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("kommentar")]
    public string? Kommentar { get; set; }
}