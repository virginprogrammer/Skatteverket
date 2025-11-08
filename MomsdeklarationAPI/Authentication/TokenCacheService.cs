using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Serilog;

namespace MomsdeklarationAPI.Authentication;

public interface ITokenCacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    Task RemoveAsync(string key);
    string GenerateCacheKey(params string[] keyParts);
}

public class TokenCacheService : ITokenCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;
    private const string CACHE_PREFIX = "momsdeklaration:token:";

    public TokenCacheService(IDistributedCache cache, ILogger logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var cachedData = await _cache.GetStringAsync(fullKey);

            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.Debug("Cache miss for key: {Key}", fullKey);
                return null;
            }

            _logger.Debug("Cache hit for key: {Key}", fullKey);
            return JsonConvert.DeserializeObject<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving from cache with key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedData = JsonConvert.SerializeObject(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(fullKey, serializedData, options);
            _logger.Debug("Cached data with key: {Key}, expiration: {Expiration}", fullKey, expiration);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error setting cache with key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _cache.RemoveAsync(fullKey);
            _logger.Debug("Removed cache entry with key: {Key}", fullKey);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error removing cache with key: {Key}", key);
        }
    }

    public string GenerateCacheKey(params string[] keyParts)
    {
        var combined = string.Join(":", keyParts.Where(k => !string.IsNullOrEmpty(k)));
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-");
    }

    private string GetFullKey(string key)
    {
        return $"{CACHE_PREFIX}{key}";
    }
}

public class CachedToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Scope { get; set; } = string.Empty;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt.AddSeconds(-30);
}