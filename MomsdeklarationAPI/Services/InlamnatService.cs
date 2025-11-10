using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Services;

public interface IInlamnatService
{
    Task<InlamnatGetResponse> GetSubmittedAsync(string redovisare, string redovisningsperiod);
    Task<InlamnatPostResponse> GetMultipleSubmittedAsync(HamtaPostMultiRequest request);
}

public class InlamnatService : IInlamnatService
{
    private readonly ISkatteverketApiClient _apiClient;
    private readonly ILogger<InlamnatService> _logger;

    public InlamnatService(ISkatteverketApiClient apiClient, ILogger<InlamnatService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<InlamnatGetResponse> GetSubmittedAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Fetching submitted declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            return await _apiClient.GetSubmittedDeclarationAsync(redovisare, redovisningsperiod);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch submitted declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<InlamnatPostResponse> GetMultipleSubmittedAsync(HamtaPostMultiRequest request)
    {
        try
        {
            _logger.Information("Fetching submitted declarations for {Count} organizations", 
                request.Redovisare.Count);
            
            return await _apiClient.GetMultipleSubmittedDeclarationsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch multiple submitted declarations");
            throw;
        }
    }
}