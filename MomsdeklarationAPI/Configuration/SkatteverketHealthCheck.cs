using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MomsdeklarationAPI.Configuration;

public class SkatteverketHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly SkatteverketApiSettings _settings;

    public SkatteverketHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<SkatteverketApiSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient("SkatteverketAPI");
        _settings = settings.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("ping", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Skatteverket API is reachable");
            }

            return HealthCheckResult.Degraded(
                $"Skatteverket API returned {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Skatteverket API is not reachable", 
                exception: ex);
        }
        catch (TaskCanceledException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Skatteverket API request timed out", 
                exception: ex);
        }
    }
}