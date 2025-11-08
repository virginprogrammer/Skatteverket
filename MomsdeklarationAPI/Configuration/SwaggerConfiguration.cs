using Microsoft.OpenApi.Models;
using System.Reflection;

namespace MomsdeklarationAPI.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Momsdeklaration API",
                Version = "v1",
                Description = "ASP.NET Core Web API for Swedish VAT Declaration (Momsdeklaration) integration with Skatteverket",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@example.com",
                    Url = new Uri("https://example.com/support")
                },
                License = new OpenApiLicense
                {
                    Name = "Private License",
                    Url = new Uri("https://example.com/license")
                },
                TermsOfService = new Uri("https://example.com/terms")
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityDefinition("Certificate", new OpenApiSecurityScheme
            {
                Description = "Client certificate authentication",
                Name = "Certificate",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Certificate"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.EnableAnnotations();
            
            c.AddServer(new OpenApiServer
            {
                Url = "/",
                Description = "Current server"
            });

            c.OperationFilter<CorrelationIdOperationFilter>();
            c.SchemaFilter<ExampleSchemaFilter>();
        });

        return services;
    }
}

public class CorrelationIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-Id",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            },
            Description = "Correlation ID for request tracking"
        });
    }
}

public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Name == "Momsuppgift")
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["momspliktigForsaljning"] = new Microsoft.OpenApi.Any.OpenApiDouble(100000),
                ["momsForsaljningUtgaendeHog"] = new Microsoft.OpenApi.Any.OpenApiDouble(25000),
                ["ingaendeMomsAvdrag"] = new Microsoft.OpenApi.Any.OpenApiDouble(5000),
                ["summaMoms"] = new Microsoft.OpenApi.Any.OpenApiDouble(20000)
            };
        }
    }
}