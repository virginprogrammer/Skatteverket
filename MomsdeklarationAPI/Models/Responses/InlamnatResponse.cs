using MomsdeklarationAPI.Models.DTOs;
using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.Responses;

public class InlamnatGetResponse
{
    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("inlamnadDatum")]
    public DateTime InlamnadDatum { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("referensnummer")]
    public string? Referensnummer { get; set; }

    [JsonProperty("kvittonummer")]
    public string? Kvittonummer { get; set; }

    [JsonProperty("kommentar")]
    public string? Kommentar { get; set; }
}

public class InlamnatPostResponse
{
    [JsonProperty("inlamnade")]
    public List<InlamnatItem> Inlamnade { get; set; } = new();
}

public class InlamnatItem
{
    [JsonProperty("redovisare")]
    public string Redovisare { get; set; } = string.Empty;

    [JsonProperty("redovisningsperiod")]
    public string Redovisningsperiod { get; set; } = string.Empty;

    [JsonProperty("momsuppgift")]
    public Momsuppgift Momsuppgift { get; set; } = new();

    [JsonProperty("inlamnadDatum")]
    public DateTime InlamnadDatum { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("referensnummer")]
    public string? Referensnummer { get; set; }

    [JsonProperty("kvittonummer")]
    public string? Kvittonummer { get; set; }
}