using MomsdeklarationAPI.Authentication;
using MomsdeklarationAPI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace MomsdeklarationAPI.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMomsdeklarationServices(this IServiceCollection services)
    {
        services.TryAddScoped<ITokenService, TokenService>();
        services.TryAddScoped<ITokenCacheService, TokenCacheService>();
        services.TryAddScoped<ISkatteverketApiClient, SkatteverketApiClient>();
        services.TryAddScoped<IMomsdeklarationService, MomsdeklarationService>();
        services.TryAddScoped<IValidationService, ValidationService>();
        services.TryAddScoped<IInlamnatService, InlamnatService>();
        services.TryAddScoped<IBeslutatService, BeslutatService>();
        services.TryAddScoped<IAuditService, AuditService>();

        return services;
    }

    public static IServiceCollection AddDistributedCacheIfNotExists(this IServiceCollection services)
    {
        if (!services.Any(x => x.ServiceType == typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache)))
        {
            services.AddMemoryCache();
            services.TryAddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache, MemoryDistributedCache>();
        }

        return services;
    }

    public static IServiceCollection AddCertificateAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var certificateSettings = configuration.GetSection("SkatteverketAPI:Certificate").Get<CertificateSettings>();
        
        if (certificateSettings != null && !string.IsNullOrEmpty(certificateSettings.Path))
        {
            services.AddAuthentication()
                .AddCertificate(options =>
                {
                    options.RequiredThumbprint = certificateSettings.Thumbprint;
                    options.RequiredIssuer = "CN=Skatteverket";
                    options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online;
                    options.AllowSelfSignedCertificates = false;
                });
        }

        return services;
    }

    public static IServiceCollection AddHealthChecksWithDependencies(this IServiceCollection services)
    {
        var rootPath = OperatingSystem.IsWindows() ? "C:\\" : "/";

        services.AddHealthChecks()
            .AddCheck<SkatteverketHealthCheck>("skatteverket_api")
            .AddMemoryHealthCheck("memory")
            .AddDiskStorageHealthCheck(options => options.AddDrive(rootPath, 1024))
            .AddProcessAllocatedMemoryHealthCheck(512);

        return services;
    }
}

public static class MemoryHealthCheckExtensions
{
    public static IHealthChecksBuilder AddMemoryHealthCheck(this IHealthChecksBuilder builder, string name)
    {
        return builder.AddCheck(name, () =>
        {
            var allocatedMemory = GC.GetTotalMemory(false);
            var memoryLimitMB = 512;
            var memoryLimitBytes = memoryLimitMB * 1024 * 1024;

            if (allocatedMemory > memoryLimitBytes)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                    $"Allocated memory ({allocatedMemory / 1024 / 1024} MB) exceeds limit ({memoryLimitMB} MB)");
            }

            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                $"Allocated memory: {allocatedMemory / 1024 / 1024} MB");
        });
    }

    public static IHealthChecksBuilder AddDiskStorageHealthCheck(this IHealthChecksBuilder builder, Action<DiskStorageOptions> configureOptions)
    {
        var options = new DiskStorageOptions();
        configureOptions(options);

        return builder.AddCheck("disk_storage", () =>
        {
            try
            {
                foreach (var drive in options.Drives)
                {
                    var driveInfo = new DriveInfo(drive.DriveName);
                    if (driveInfo.AvailableFreeSpace < drive.MinimumFreeMB * 1024 * 1024)
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                            $"Drive {drive.DriveName} has insufficient free space");
                    }
                }

                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("All drives have sufficient space");
            }
            catch (Exception ex)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Disk check failed", ex);
            }
        });
    }

    public static IHealthChecksBuilder AddProcessAllocatedMemoryHealthCheck(this IHealthChecksBuilder builder, long maxMemoryMB)
    {
        return builder.AddCheck("process_memory", () =>
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var maxMemoryBytes = maxMemoryMB * 1024 * 1024;

            if (workingSet > maxMemoryBytes)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                    $"Process memory ({workingSet / 1024 / 1024} MB) exceeds limit ({maxMemoryMB} MB)");
            }

            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(
                $"Process memory: {workingSet / 1024 / 1024} MB");
        });
    }
}

public class DiskStorageOptions
{
    public List<DriveConfig> Drives { get; set; } = new();

    public DiskStorageOptions AddDrive(string driveName, long minimumFreeMB)
    {
        Drives.Add(new DriveConfig { DriveName = driveName, MinimumFreeMB = minimumFreeMB });
        return this;
    }
}

public class DriveConfig
{
    public string DriveName { get; set; } = string.Empty;
    public long MinimumFreeMB { get; set; }
}