using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using MomsdeklarationAPI.Configuration;
using MomsdeklarationAPI.Middleware;
using Serilog;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting MomsdeklarationAPI");
    
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
        });
    
    builder.Services.AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    builder.Services.Configure<SkatteverketApiSettings>(
        builder.Configuration.GetSection("SkatteverketAPI"));
    
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedCacheIfNotExists();
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys")))
        .SetApplicationName("MomsdeklarationAPI");
    
    builder.Services.AddMomsdeklarationServices();
    builder.Services.AddCertificateAuthentication(builder.Configuration);
    
    builder.Services.AddHttpClient("SkatteverketAPI", (serviceProvider, client) =>
    {
        var settings = builder.Configuration.GetSection("SkatteverketAPI").Get<SkatteverketApiSettings>();
        var baseUrl = settings?.UseTestEnvironment == true ? settings.TestBaseUrl : settings?.BaseUrl;
        
        if (!string.IsNullOrEmpty(baseUrl))
        {
            client.BaseAddress = new Uri(baseUrl);
        }
        
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Accept-Language", "sv-SE");
        client.Timeout = TimeSpan.FromSeconds(settings?.Timeout ?? 30);
    })
    .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
    {
        var handler = new HttpClientHandler();
        var settings = builder.Configuration.GetSection("SkatteverketAPI").Get<SkatteverketApiSettings>();
        
        if (settings?.Certificate != null && !string.IsNullOrEmpty(settings.Certificate.Path))
        {
            try
            {
                X509Certificate2? certificate = null;
                
                if (!string.IsNullOrEmpty(settings.Certificate.Path) && File.Exists(settings.Certificate.Path))
                {
                    certificate = new X509Certificate2(
                        settings.Certificate.Path,
                        settings.Certificate.Password,
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                }
                else if (!string.IsNullOrEmpty(settings.Certificate.Thumbprint))
                {
                    using var store = new X509Store(
                        Enum.Parse<StoreName>(settings.Certificate.StoreName ?? "My"),
                        Enum.Parse<StoreLocation>(settings.Certificate.StoreLocation ?? "CurrentUser"));
                    
                    store.Open(OpenFlags.ReadOnly);
                    var certificates = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        settings.Certificate.Thumbprint,
                        false);
                    
                    if (certificates.Count > 0)
                    {
                        certificate = certificates[0];
                    }
                }
                
                if (certificate != null)
                {
                    handler.ClientCertificates.Add(certificate);
                    Log.Information("Certificate configured for API client");
                }
                else
                {
                    Log.Warning("No certificate found for API client");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to configure certificate for API client");
            }
        }
        
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true;
        
        return handler;
    })
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());
    
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var settings = builder.Configuration.GetSection("SkatteverketAPI").Get<SkatteverketApiSettings>();
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Error(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Debug("Token validated successfully");
                    return Task.CompletedTask;
                }
            };
        });
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });
    
    builder.Services.AddHealthChecksWithDependencies();
    
    builder.Services.AddHttpContextAccessor();
    
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });
    
    var app = builder.Build();
    
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Momsdeklaration API v1");
            c.RoutePrefix = string.Empty;
        });
    }
    
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseRateLimiting(new RateLimitOptions { MaxRequests = 100, TimeWindow = TimeSpan.FromMinutes(15) });
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<AuditMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    
    app.UseHttpsRedirection();
    
    app.UseCors("AllowSpecificOrigins");
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();
    
    app.MapHealthChecks("/health");
    
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
    
    Log.Information("MomsdeklarationAPI started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public for testing
public partial class Program { }