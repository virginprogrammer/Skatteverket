using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MomsdeklarationAPI.Models.DTOs;

public class Momsuppgift
{
    [JsonProperty("momspliktigForsaljning")]
    public decimal? MomspliktigForsaljning { get; set; }

    [JsonProperty("momspliktigaUttag")]
    public decimal? MomspliktigaUttag { get; set; }

    [JsonProperty("vinstmarginal")]
    public decimal? Vinstmarginal { get; set; }

    [JsonProperty("hyresInkomst")]
    public decimal? HyresInkomst { get; set; }

    [JsonProperty("momsForsaljningUtgaendeHog")]
    public decimal? MomsForsaljningUtgaendeHog { get; set; }

    [JsonProperty("momsForsaljningUtgaendeMedel")]
    public decimal? MomsForsaljningUtgaendeMedel { get; set; }

    [JsonProperty("momsForsaljningUtgaendeLag")]
    public decimal? MomsForsaljningUtgaendeLag { get; set; }

    [JsonProperty("inkopVarorEU")]
    public decimal? InkopVarorEU { get; set; }

    [JsonProperty("inkopTjansterEU")]
    public decimal? InkopTjansterEU { get; set; }

    [JsonProperty("inkopTjansterUtanforEU")]
    public decimal? InkopTjansterUtanforEU { get; set; }

    [JsonProperty("inkopVarorSE")]
    public decimal? InkopVarorSE { get; set; }

    [JsonProperty("inkopTjansterSE")]
    public decimal? InkopTjansterSE { get; set; }

    [JsonProperty("momsInkopUtgaendeHog")]
    public decimal? MomsInkopUtgaendeHog { get; set; }

    [JsonProperty("momsInkopUtgaendeMedel")]
    public decimal? MomsInkopUtgaendeMedel { get; set; }

    [JsonProperty("momsInkopUtgaendeLag")]
    public decimal? MomsInkopUtgaendeLag { get; set; }

    [JsonProperty("forsaljningVarorEU")]
    public decimal? ForsaljningVarorEU { get; set; }

    [JsonProperty("forsaljningVarorUtanforEU")]
    public decimal? ForsaljningVarorUtanforEU { get; set; }

    [JsonProperty("inkopVaror3pHandel")]
    public decimal? InkopVaror3pHandel { get; set; }

    [JsonProperty("forsaljningVaror3pHandel")]
    public decimal? ForsaljningVaror3pHandel { get; set; }

    [JsonProperty("forsaljningTjansterEU")]
    public decimal? ForsaljningTjansterEU { get; set; }

    [JsonProperty("ovrigForsaljningTjansterUtanforSE")]
    public decimal? OvrigForsaljningTjansterUtanforSE { get; set; }

    [JsonProperty("forsaljningBskKopareSE")]
    public decimal? ForsaljningBskKopareSE { get; set; }

    [JsonProperty("momsfriForsaljning")]
    public decimal? MomsfriForsaljning { get; set; }

    [JsonProperty("import")]
    public decimal? Import { get; set; }

    [JsonProperty("momsImportUtgaendeHog")]
    public decimal? MomsImportUtgaendeHog { get; set; }

    [JsonProperty("momsImportUtgaendeMedel")]
    public decimal? MomsImportUtgaendeMedel { get; set; }

    [JsonProperty("momsImportUtgaendeLag")]
    public decimal? MomsImportUtgaendeLag { get; set; }

    [JsonProperty("ingaendeMomsAvdrag")]
    public decimal? IngaendeMomsAvdrag { get; set; }

    [JsonProperty("summaMoms")]
    public decimal? SummaMoms { get; set; }
}