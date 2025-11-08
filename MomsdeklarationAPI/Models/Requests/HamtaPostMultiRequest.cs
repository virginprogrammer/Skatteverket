using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Requests;

public class HamtaPostMultiRequest
{
    [JsonProperty("redovisare")]
    public List<string> Redovisare { get; set; } = new();

    [JsonProperty("redovisningsperiod")]
    public string? Redovisningsperiod { get; set; }

    [JsonProperty("frånOchMedRedovisningsperiod")]
    public string? FranOchMedRedovisningsperiod { get; set; }

    [JsonProperty("tillOchMedRedovisningsperiod")]
    public string? TillOchMedRedovisningsperiod { get; set; }
}