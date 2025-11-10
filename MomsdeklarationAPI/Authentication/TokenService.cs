using System.Net.Http.Headers;
using System.Text;
using MomsdeklarationAPI.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MomsdeklarationAPI.Authentication;

public interface ITokenService
{
    Task<string> GetAccessTokenAsync();
    Task<TokenResponse?> RequestTokenAsync();
}

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly SkatteverketApiSettings _settings;
    private readonly ILogger<TokenService> _logger;
    private const string TOKEN_CACHE_KEY = "skatteverket_access_token";

    public TokenService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<SkatteverketApiSettings> settings,
        ILogger<TokenService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("SkatteverketAPI");
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cache.TryGetValue(TOKEN_CACHE_KEY, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.Debug("Using cached access token");
            return cachedToken;
        }

        _logger.Information("Requesting new access token");
        var tokenResponse = await RequestTokenAsync();

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Failed to obtain access token");
        }

        var expiresIn = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600;
        var cacheExpiry = TimeSpan.FromSeconds(expiresIn - 60);

        _cache.Set(TOKEN_CACHE_KEY, tokenResponse.AccessToken, cacheExpiry);
        _logger.Information("Access token cached for {CacheExpiry} seconds", cacheExpiry.TotalSeconds);

        return tokenResponse.AccessToken;
    }

    public async Task<TokenResponse?> RequestTokenAsync()
    {
        try
        {
            var tokenEndpoint = _settings.UseTestEnvironment
                ? _settings.OAuth.TestTokenEndpoint
                : _settings.OAuth.TokenEndpoint;

            if (string.IsNullOrEmpty(tokenEndpoint))
            {
                _logger.Error("Token endpoint not configured");
                throw new InvalidOperationException("Token endpoint not configured");
            }

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", _settings.OAuth.GrantType),
                new KeyValuePair<string, string>("client_id", _settings.OAuth.ClientId),
                new KeyValuePair<string, string>("client_secret", _settings.OAuth.ClientSecret),
                new KeyValuePair<string, string>("scope", _settings.OAuth.Scope)
            });

            tokenRequest.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            _logger.Debug("Requesting token from {TokenEndpoint}", tokenEndpoint);
            
            var response = await _httpClient.PostAsync(tokenEndpoint, tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Token request failed with status {StatusCode}: {Content}", 
                    response.StatusCode, content);
                throw new HttpRequestException($"Token request failed: {response.StatusCode}");
            }

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);
            
            if (tokenResponse != null)
            {
                _logger.Information("Successfully obtained access token");
            }

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to request access token");
            throw;
        }
    }
}

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; } = string.Empty;
}