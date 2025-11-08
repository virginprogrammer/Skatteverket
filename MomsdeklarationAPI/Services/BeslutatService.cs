using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Serilog;

namespace MomsdeklarationAPI.Services;

public interface IBeslutatService
{
    Task<BeslutatGetResponse> GetDecidedAsync(string redovisare, string redovisningsperiod);
    Task<BeslutatPostResponse> GetMultipleDecidedAsync(HamtaPostMultiRequest request);
}

public class BeslutatService : IBeslutatService
{
    private readonly ISkatteverketApiClient _apiClient;
    private readonly ILogger _logger;

    public BeslutatService(ISkatteverketApiClient apiClient, ILogger logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<BeslutatGetResponse> GetDecidedAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            _logger.Information("Fetching decided declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            
            return await _apiClient.GetDecidedDeclarationAsync(redovisare, redovisningsperiod);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch decided declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<BeslutatPostResponse> GetMultipleDecidedAsync(HamtaPostMultiRequest request)
    {
        try
        {
            _logger.Information("Fetching decided declarations for {Count} organizations", 
                request.Redovisare.Count);
            
            return await _apiClient.GetMultipleDecidedDeclarationsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch multiple decided declarations");
            throw;
        }
    }
}