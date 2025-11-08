using Microsoft.AspNetCore.Mvc;

namespace MomsdeklarationAPI.Configuration;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioning(this IServiceCollection services, Action<ApiVersioningOptions> setupAction)
    {
        services.Configure(setupAction);
        return services;
    }
}

public class ApiVersioningOptions
{
    public ApiVersion DefaultApiVersion { get; set; } = new ApiVersion(1, 0);
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;
    public bool ReportApiVersions { get; set; } = true;
}