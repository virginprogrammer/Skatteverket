using FluentValidation;
using MomsdeklarationAPI.Models.DTOs;

namespace MomsdeklarationAPI.Models.Validators;

public class MomsuppgiftValidator : AbstractValidator<Momsuppgift>
{
    public MomsuppgiftValidator()
    {
        RuleFor(x => x.MomspliktigForsaljning)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MomspliktigForsaljning.HasValue)
            .WithMessage("Momspliktig försäljning cannot be negative");

        RuleFor(x => x.MomsForsaljningUtgaendeHog)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MomsForsaljningUtgaendeHog.HasValue)
            .WithMessage("Utgående moms 25% cannot be negative");

        RuleFor(x => x.MomsForsaljningUtgaendeMedel)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MomsForsaljningUtgaendeMedel.HasValue)
            .WithMessage("Utgående moms 12% cannot be negative");

        RuleFor(x => x.MomsForsaljningUtgaendeLag)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MomsForsaljningUtgaendeLag.HasValue)
            .WithMessage("Utgående moms 6% cannot be negative");

        RuleFor(x => x.IngaendeMomsAvdrag)
            .GreaterThanOrEqualTo(0)
            .When(x => x.IngaendeMomsAvdrag.HasValue)
            .WithMessage("Ingående moms avdrag cannot be negative");

        RuleFor(x => x)
            .Must(ValidateTotalVat)
            .WithMessage("Total VAT calculation is incorrect")
            .WithName("SummaMoms");

        RuleFor(x => x)
            .Must(ValidateEuPurchaseVat)
            .WithMessage("EU purchase VAT must be reported when EU purchases exist")
            .WithName("EuPurchaseValidation");

        RuleFor(x => x)
            .Must(ValidateImportVat)
            .WithMessage("Import VAT must be reported when imports exist")
            .WithName("ImportValidation");
    }

    private bool ValidateTotalVat(Momsuppgift momsuppgift)
    {
        if (!momsuppgift.SummaMoms.HasValue)
            return true;

        decimal totalUtgaende = CalculateTotalUtgaendeMoms(momsuppgift);
        decimal ingaende = momsuppgift.IngaendeMomsAvdrag ?? 0;
        decimal calculated = totalUtgaende - ingaende;

        return Math.Abs(calculated - momsuppgift.SummaMoms.Value) <= 1;
    }

    private bool ValidateEuPurchaseVat(Momsuppgift momsuppgift)
    {
        bool hasEuPurchases = (momsuppgift.InkopVarorEU ?? 0) > 0 || 
                              (momsuppgift.InkopTjansterEU ?? 0) > 0;

        if (!hasEuPurchases)
            return true;

        bool hasEuVat = (momsuppgift.MomsInkopUtgaendeHog ?? 0) > 0 ||
                        (momsuppgift.MomsInkopUtgaendeMedel ?? 0) > 0 ||
                        (momsuppgift.MomsInkopUtgaendeLag ?? 0) > 0;

        return hasEuVat;
    }

    private bool ValidateImportVat(Momsuppgift momsuppgift)
    {
        bool hasImport = (momsuppgift.Import ?? 0) > 0;

        if (!hasImport)
            return true;

        bool hasImportVat = (momsuppgift.MomsImportUtgaendeHog ?? 0) > 0 ||
                           (momsuppgift.MomsImportUtgaendeMedel ?? 0) > 0 ||
                           (momsuppgift.MomsImportUtgaendeLag ?? 0) > 0;

        return hasImportVat;
    }

    private decimal CalculateTotalUtgaendeMoms(Momsuppgift momsuppgift)
    {
        return (momsuppgift.MomsForsaljningUtgaendeHog ?? 0) +
               (momsuppgift.MomsForsaljningUtgaendeMedel ?? 0) +
               (momsuppgift.MomsForsaljningUtgaendeLag ?? 0) +
               (momsuppgift.MomsInkopUtgaendeHog ?? 0) +
               (momsuppgift.MomsInkopUtgaendeMedel ?? 0) +
               (momsuppgift.MomsInkopUtgaendeLag ?? 0) +
               (momsuppgift.MomsImportUtgaendeHog ?? 0) +
               (momsuppgift.MomsImportUtgaendeMedel ?? 0) +
               (momsuppgift.MomsImportUtgaendeLag ?? 0);
    }
}