using MomsdeklarationAPI.Models.DTOs;

namespace MomsdeklarationAPI.Services;

public interface IValidationService
{
    Task<ValidationResult> ValidateMomsuppgiftAsync(Momsuppgift momsuppgift);
    ValidationResult ValidateRedovisare(string redovisare);
    ValidationResult ValidateRedovisningsperiod(string redovisningsperiod);
}

public class ValidationService : IValidationService
{
    public Task<ValidationResult> ValidateMomsuppgiftAsync(Momsuppgift momsuppgift)
    {
        var result = new ValidationResult();

        if (momsuppgift == null)
        {
            result.Errors.Add("Momsuppgift cannot be null");
            return Task.FromResult(result);
        }

        decimal totalUtgaendeMoms = 
            (momsuppgift.MomsForsaljningUtgaendeHog ?? 0) +
            (momsuppgift.MomsForsaljningUtgaendeMedel ?? 0) +
            (momsuppgift.MomsForsaljningUtgaendeLag ?? 0) +
            (momsuppgift.MomsInkopUtgaendeHog ?? 0) +
            (momsuppgift.MomsInkopUtgaendeMedel ?? 0) +
            (momsuppgift.MomsInkopUtgaendeLag ?? 0) +
            (momsuppgift.MomsImportUtgaendeHog ?? 0) +
            (momsuppgift.MomsImportUtgaendeMedel ?? 0) +
            (momsuppgift.MomsImportUtgaendeLag ?? 0);

        decimal ingaendeMoms = momsuppgift.IngaendeMomsAvdrag ?? 0;
        decimal calculatedSummaMoms = totalUtgaendeMoms - ingaendeMoms;

        if (momsuppgift.SummaMoms.HasValue)
        {
            decimal diff = Math.Abs(calculatedSummaMoms - momsuppgift.SummaMoms.Value);
            if (diff > 1)
            {
                result.Errors.Add($"SummaMoms calculation mismatch. Expected: {calculatedSummaMoms}, Actual: {momsuppgift.SummaMoms.Value}");
            }
        }

        ValidateNegativeValues(momsuppgift, result);
        ValidateBusinessRules(momsuppgift, result);

        result.IsValid = !result.Errors.Any();
        return Task.FromResult(result);
    }

    public ValidationResult ValidateRedovisare(string redovisare)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(redovisare))
        {
            result.Errors.Add("Redovisare is required");
        }
        else if (redovisare.Length != 10 && redovisare.Length != 12)
        {
            result.Errors.Add("Redovisare must be a valid Swedish organization number (10 or 12 digits)");
        }
        else if (!IsValidOrganizationNumber(redovisare))
        {
            result.Errors.Add("Invalid Swedish organization number format");
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    public ValidationResult ValidateRedovisningsperiod(string redovisningsperiod)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(redovisningsperiod))
        {
            result.Errors.Add("Redovisningsperiod is required");
        }
        else if (redovisningsperiod.Length != 6)
        {
            result.Errors.Add("Redovisningsperiod must be in format YYYYMM");
        }
        else
        {
            if (int.TryParse(redovisningsperiod.Substring(0, 4), out int year) &&
                int.TryParse(redovisningsperiod.Substring(4, 2), out int month))
            {
                if (year < 2000 || year > DateTime.Now.Year + 1)
                {
                    result.Errors.Add($"Invalid year in redovisningsperiod: {year}");
                }
                if (month < 1 || month > 12)
                {
                    result.Errors.Add($"Invalid month in redovisningsperiod: {month}");
                }
            }
            else
            {
                result.Errors.Add("Redovisningsperiod must be numeric in format YYYYMM");
            }
        }

        result.IsValid = !result.Errors.Any();
        return result;
    }

    private void ValidateNegativeValues(Momsuppgift momsuppgift, ValidationResult result)
    {
        var properties = typeof(Momsuppgift).GetProperties()
            .Where(p => p.PropertyType == typeof(decimal?) && p.Name != "SummaMoms");

        foreach (var prop in properties)
        {
            var value = prop.GetValue(momsuppgift) as decimal?;
            if (value.HasValue && value.Value < 0 && prop.Name != "IngaendeMomsAvdrag")
            {
                result.Warnings.Add($"{prop.Name} has a negative value: {value.Value}");
            }
        }
    }

    private void ValidateBusinessRules(Momsuppgift momsuppgift, ValidationResult result)
    {
        if (momsuppgift.MomspliktigForsaljning.HasValue && momsuppgift.MomspliktigForsaljning > 0)
        {
            bool hasUtgaendeMoms = 
                momsuppgift.MomsForsaljningUtgaendeHog > 0 ||
                momsuppgift.MomsForsaljningUtgaendeMedel > 0 ||
                momsuppgift.MomsForsaljningUtgaendeLag > 0;

            if (!hasUtgaendeMoms)
            {
                result.Warnings.Add("Momspliktig försäljning exists but no utgående moms reported");
            }
        }

        if ((momsuppgift.InkopVarorEU > 0 || momsuppgift.InkopTjansterEU > 0) &&
            !(momsuppgift.MomsInkopUtgaendeHog > 0 || 
              momsuppgift.MomsInkopUtgaendeMedel > 0 || 
              momsuppgift.MomsInkopUtgaendeLag > 0))
        {
            result.Warnings.Add("EU purchases reported but no corresponding VAT");
        }

        if (momsuppgift.Import > 0 &&
            !(momsuppgift.MomsImportUtgaendeHog > 0 || 
              momsuppgift.MomsImportUtgaendeMedel > 0 || 
              momsuppgift.MomsImportUtgaendeLag > 0))
        {
            result.Warnings.Add("Import reported but no import VAT");
        }
    }

    private bool IsValidOrganizationNumber(string orgNumber)
    {
        orgNumber = orgNumber.Replace("-", "").Replace(" ", "");
        
        if (orgNumber.Length == 12)
        {
            orgNumber = orgNumber.Substring(2);
        }

        if (orgNumber.Length != 10 || !orgNumber.All(char.IsDigit))
        {
            return false;
        }

        int checkSum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = int.Parse(orgNumber[i].ToString());
            int multiplier = (i % 2 == 0) ? 2 : 1;
            int product = digit * multiplier;
            
            if (product > 9)
            {
                product = (product / 10) + (product % 10);
            }
            
            checkSum += product;
        }

        int checkDigit = (10 - (checkSum % 10)) % 10;
        return checkDigit == int.Parse(orgNumber[9].ToString());
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}