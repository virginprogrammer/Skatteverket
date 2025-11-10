using System.Net.Http.Headers;
using System.Text;
using MomsdeklarationAPI.Authentication;
using MomsdeklarationAPI.Configuration;
using MomsdeklarationAPI.Models.DTOs;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Services;

public interface ISkatteverketApiClient
{
    Task<UtkastResponse> CreateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request, bool lockDraft = false);
    Task<UtkastGetResponse> GetDraftAsync(string redovisare, string redovisningsperiod);
    Task<bool> DeleteDraftAsync(string redovisare, string redovisningsperiod);
    Task<UtkastPostMultiResponse> GetMultipleDraftsAsync(HamtaPostMultiRequest request);
    Task<KontrollResultat> ValidateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request);
    Task<bool> LockDraftAsync(string redovisare, string redovisningsperiod);
    Task<bool> UnlockDraftAsync(string redovisare, string redovisningsperiod);
}

public class SkatteverketApiClient : ISkatteverketApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly SkatteverketApiSettings _settings;
    private readonly ILogger<SkatteverketApiClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SkatteverketApiClient(
        IHttpClientFactory httpClientFactory,
        ITokenService tokenService,
        IOptions<SkatteverketApiSettings> settings,
        ILogger<SkatteverketApiClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("SkatteverketAPI");
        _tokenService = tokenService;
        _settings = settings.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UtkastResponse> CreateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request, bool lockDraft = false)
    {
        try
        {
            var endpoint = $"utkast/{redovisare}/{redovisningsperiod}";
            if (lockDraft)
            {
                endpoint += "?lock=true";
            }

            var response = await SendRequestAsync<UtkastPostRequest, UtkastResponse>(
                HttpMethod.Post, endpoint, request);

            return response ?? new UtkastResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<UtkastGetResponse> GetDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"utkast/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync<object, UtkastGetResponse>(
                HttpMethod.Get, endpoint, null);

            return response ?? new UtkastGetResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> DeleteDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"utkast/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync(HttpMethod.Delete, endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<UtkastPostMultiResponse> GetMultipleDraftsAsync(HamtaPostMultiRequest request)
    {
        try
        {
            var response = await SendRequestAsync<HamtaPostMultiRequest, UtkastPostMultiResponse>(
                HttpMethod.Post, "utkast", request);

            return response ?? new UtkastPostMultiResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get multiple drafts");
            throw;
        }
    }

    public async Task<KontrollResultat> ValidateDraftAsync(string redovisare, string redovisningsperiod, UtkastPostRequest request)
    {
        try
        {
            var endpoint = $"kontrollera/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync<UtkastPostRequest, KontrollResultat>(
                HttpMethod.Post, endpoint, request);

            return response ?? new KontrollResultat();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> LockDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"las/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync(HttpMethod.Put, endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to lock draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<bool> UnlockDraftAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"las/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync(HttpMethod.Delete, endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlock draft for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<InlamnatGetResponse> GetSubmittedDeclarationAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"inlamnat/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync<object, InlamnatGetResponse>(
                HttpMethod.Get, endpoint, null);

            return response ?? new InlamnatGetResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submitted declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<InlamnatPostResponse> GetMultipleSubmittedDeclarationsAsync(HamtaPostMultiRequest request)
    {
        try
        {
            var response = await SendRequestAsync<HamtaPostMultiRequest, InlamnatPostResponse>(
                HttpMethod.Post, "inlamnat", request);

            return response ?? new InlamnatPostResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get multiple submitted declarations");
            throw;
        }
    }

    public async Task<BeslutatGetResponse> GetDecidedDeclarationAsync(string redovisare, string redovisningsperiod)
    {
        try
        {
            var endpoint = $"beslutat/{redovisare}/{redovisningsperiod}";
            var response = await SendRequestAsync<object, BeslutatGetResponse>(
                HttpMethod.Get, endpoint, null);

            return response ?? new BeslutatGetResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get decided declaration for {Redovisare}/{Redovisningsperiod}", 
                redovisare, redovisningsperiod);
            throw;
        }
    }

    public async Task<BeslutatPostResponse> GetMultipleDecidedDeclarationsAsync(HamtaPostMultiRequest request)
    {
        try
        {
            var response = await SendRequestAsync<HamtaPostMultiRequest, BeslutatPostResponse>(
                HttpMethod.Post, "beslutat", request);

            return response ?? new BeslutatPostResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get multiple decided declarations");
            throw;
        }
    }

    private async Task<TResponse?> SendRequestAsync<TRequest, TResponse>(
        HttpMethod method, string endpoint, TRequest? requestData)
        where TResponse : class
    {
        var response = await SendRequestAsync(method, endpoint, requestData);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        await HandleErrorResponse(response);
        return null;
    }

    private async Task<HttpResponseMessage> SendRequestAsync<TRequest>(
        HttpMethod method, string endpoint, TRequest? requestData = default)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        await AddAuthenticationHeader(request);
        AddCustomHeaders(request);

        if (requestData != null && (method == HttpMethod.Post || method == HttpMethod.Put))
        {
            var json = JsonConvert.SerializeObject(requestData);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        _logger.LogDebug("Sending {Method} request to {Endpoint}", method, endpoint);
        
        var response = await _httpClient.SendAsync(request);
        
        _logger.LogDebug("Received response {StatusCode} from {Endpoint}", 
            response.StatusCode, endpoint);

        return response;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string endpoint)
    {
        return await SendRequestAsync<object>(method, endpoint, null);
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage request)
    {
        var token = await _tokenService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private void AddCustomHeaders(HttpRequestMessage request)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() 
            ?? Guid.NewGuid().ToString();
        
        request.Headers.Add("skv_client_correlation_id", correlationId);
        request.Headers.Add("skv_call_timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        
        if (_httpContextAccessor.HttpContext?.User?.Identity?.Name != null)
        {
            request.Headers.Add("skv_user", _httpContextAccessor.HttpContext.User.Identity.Name);
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        _logger.LogError("API request failed with status {StatusCode}: {Content}", 
            response.StatusCode, content);

        var errorMessage = $"API request failed with status {response.StatusCode}";
        
        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(content);
                if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.Message))
                {
                    errorMessage = errorResponse.Message;
                }
            }
            catch
            {
                errorMessage += $": {content}";
            }
        }

        throw new HttpRequestException(errorMessage);
    }
}