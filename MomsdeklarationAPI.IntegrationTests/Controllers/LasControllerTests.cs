using System.Net;
using FluentAssertions;
using Moq;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class LasControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public LasControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task LockDraft_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.LockDraftAsync(redovisare, period))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PutAsync($"/api/las/{redovisare}/{period}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LockDraft_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var invalidRedovisare = "abc";
        var period = "202401";

        // Act
        var response = await _client.PutAsync($"/api/las/{invalidRedovisare}/{period}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LockDraft_WithInvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        var redovisare = "5555555555";
        var invalidPeriod = "invalid";

        // Act
        var response = await _client.PutAsync($"/api/las/{redovisare}/{invalidPeriod}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnlockDraft_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.UnlockDraftAsync(redovisare, period))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/las/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnlockDraft_WhenFails_ReturnsNotFound()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.UnlockDraftAsync(redovisare, period))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/las/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LockDraft_ReturnsJsonContentType()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.LockDraftAsync(redovisare, period))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PutAsync($"/api/las/{redovisare}/{period}", null);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
