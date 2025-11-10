using MomsdeklarationAPI.Models.DTOs;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Services;

public interface IMomsdeklarationService
{
    Task<UtkastResponse> CreateOrUpdateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request, bool lockDraft = false);
    Task<UtkastGetResponse> GetDraftAsync(string redovisare, string redovisningsperiod);
    Task<bool> DeleteDraftAsync(string redovisare, string redovisningsperiod);
    Task<UtkastPostMultiResponse> GetMultipleDraftsAsync(HamtaPostMultiRequest request);
    Task<KontrollResultat> ValidateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request);
    Task<bool> LockDraftForSigningAsync(string redovisare, string redovisningsperiod);
    Task<bool> UnlockDraftAsync(string redovisare, string redovisningsperiod);
}

public class MomsdeklarationService : IMomsdeklarationService
{
    private readonly ISkatteverketApiClient _apiClient;
    private readonly IValidationService _validationService;
    private readonly ILogger<MomsdeklarationService> _logger;

    public MomsdeklarationService(
        ISkatteverketApiClient apiClient,
        IValidationService validationService,
        ILogger<MomsdeklarationService> logger)
    {
        _apiClient = apiClient;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<UtkastResponse> CreateOrUpdateDraftAsync(
        string redovisare, 
        string redovisningsperiod, 
        UtkastPostRequest request, 
        bool lockDraft = false)
    {
        try
        {
            _logger.Information("Creating/updating draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);

            var validationResult = await _validationService.ValidateMomsuppgiftAsync(request.Momsuppgift);
            if (!validationResult.IsValid)
            {
                _logger.Warning("Validation failed for draft: {Errors}", 
                    string.Join(", ", validationResult.Errors));
            }

            var response = await _apiClient.CreateDraftAsync(redovisare, redovisningsperiod, request, lockDraft);

            if (response.Sparad)
            {
                _logger.Information("Draft successfully saved for {Redovisare}/{Redovisningsperiod}", 
                    redovisare, redovisningsperiod);
            }
            else
            {
                _logger.Warning("Draft not saved for {Redovisare}/{Redovisningsperiod}. Status: {Status}", 
                    redovisare, redovisningsperiod, response.KontrollResultat?.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create/update draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<UtkastGetResponse> GetDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Fetching draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            return await _apiClient.GetDraftAsync(redovisare, redovisningsperiod);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> DeleteDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Deleting draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            var result = await _apiClient.DeleteDraftAsync(redovisare, redovisningsperiod);
            
            if (result)
            {
                _logger.Information("Draft successfully deleted for {Redovisare}/{Redovisningsperiod}", 
                    redovisare, redovisningsperiod);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<UtkastPostMultiResponse> GetMultipleDraftsAsync(HamtaPostMultiRequest request)
    {
        try
        {
            _logger.Information("Fetching drafts for {Count} organizations", request.Redovisare.Count);
            
            return await _apiClient.GetMultipleDraftsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch multiple drafts");
            throw;
        }
    }

    public async Task<KontrollResultat> ValidateDraftAsync(
        string redovisare, 
        string redovisningsperiod, 
        UtkastPostRequest request)
    {
        try
        {
            _logger.Information("Validating draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);

            var localValidation = await _validationService.ValidateMomsuppgiftAsync(request.Momsuppgift);
            
            var apiValidation = await _apiClient.ValidateDraftAsync(redovisare, redovisningsperiod, request);
            
            if (localValidation.Errors.Any())
            {
                foreach (var error in localValidation.Errors)
                {
                    apiValidation.Resultat.Add(new Kontroll
                    {
                        Typ = "VARNING",
                        Meddelande = error,
                        Kod = "LOCAL_VALIDATION"
                    });
                }
            }
            
            return apiValidation;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to validate draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> LockDraftForSigningAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Locking draft for signing: {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            var result = await _apiClient.LockDraftAsync(redovisare, redovisningsperiod);
            
            if (result)
            {
                _logger.Information("Draft successfully locked for {Redovisare}/{Redovisningsperiod}", 
                    redovisare, redovisningsperiod);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to lock draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> UnlockDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Unlocking draft: {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            var result = await _apiClient.UnlockDraftAsync(redovisare, redovisningsperiod);
            
            if (result)
            {
                _logger.Information("Draft successfully unlocked for {Redovisare}/{Redovisningsperiod}", 
                    redovisare, redovisningsperiod);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to unlock draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }
}