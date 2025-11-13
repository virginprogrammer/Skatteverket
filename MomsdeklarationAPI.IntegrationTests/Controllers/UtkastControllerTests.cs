using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Moq;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class UtkastControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public UtkastControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task GetMultipleDrafts_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string> { "5555555555", "1234567890" }
        };

        var expectedResponse = new UtkastPostMultiResponse
        {
            Utkast = new List<UtkastItem>()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetMultipleDraftsAsync(It.IsAny<HamtaPostMultiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/utkast", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task GetMultipleDrafts_WithEmptyRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/utkast", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Contain("Invalid request data");
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task GetDraft_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        var expectedResponse = new UtkastGetResponse
        {
            Momsuppgift = new Models.DTOs.Momsuppgift()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetDraftAsync(redovisare, period))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.GetAsync($"/api/utkast/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task GetDraft_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var invalidRedovisare = "123"; // Too short
        var period = "202401";

        // Act
        var response = await _client.GetAsync($"/api/utkast/{invalidRedovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task CreateOrUpdateDraft_WithValidData_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";
        var request = new UtkastPostRequest
        {
            Momsuppgift = new Models.DTOs.Momsuppgift()
        };

        var expectedResponse = new UtkastResponse
        {
            Sparad = true,
            Last = false
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.CreateDraftAsync(redovisare, period, It.IsAny<UtkastPostRequest>(), false))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/utkast/{redovisare}/{period}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task DeleteDraft_WithValidParameters_ReturnsNoContent()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.DeleteDraftAsync(redovisare, period))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync($"/api/utkast/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(Skip = "Integration test - run manually")]
    public async Task DeleteDraft_WhenDraftNotFound_ReturnsNotFound()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        _factory.MockSkatteverketApiClient
            .Setup(x => x.DeleteDraftAsync(redovisare, period))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync($"/api/utkast/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
