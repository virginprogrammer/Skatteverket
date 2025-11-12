using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MomsdeklarationAPI.Services;
using Moq;

namespace MomsdeklarationAPI.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<ISkatteverketApiClient> MockSkatteverketApiClient { get; } = new Mock<ISkatteverketApiClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the real ISkatteverketApiClient service
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISkatteverketApiClient));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add mock ISkatteverketApiClient
            services.AddScoped(_ => MockSkatteverketApiClient.Object);

            // Configure test authentication
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        });

        builder.UseEnvironment("Testing");
    }
}
