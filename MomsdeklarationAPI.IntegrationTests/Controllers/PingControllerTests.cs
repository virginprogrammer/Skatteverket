using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MomsdeklarationAPI.Controllers;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class PingControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public PingControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Ping_ReturnsOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/ping");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ping_ReturnsValidPingResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/ping");
        var pingResponse = await response.Content.ReadFromJsonAsync<PingResponse>();

        // Assert
        pingResponse.Should().NotBeNull();
        pingResponse!.Status.Should().Be("OK");
        pingResponse.Service.Should().Be("MomsdeklarationAPI");
        pingResponse.Version.Should().Be("1.0.0");
        pingResponse.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Ping_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/ping");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
