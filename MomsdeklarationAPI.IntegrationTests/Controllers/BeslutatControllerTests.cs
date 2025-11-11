using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Moq;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class BeslutatControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public BeslutatControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task GetMultipleDecided_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string> { "5555555555", "1234567890" }
        };

        var expectedResponse = new BeslutatPostResponse
        {
            Beslutade = new List<BeslutatItem>()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetMultipleDecidedDeclarationsAsync(It.IsAny<HamtaPostMultiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/beslutat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMultipleDecided_WithEmptyRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/beslutat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Contain("Invalid request data");
    }

    [Fact]
    public async Task GetMultipleDecided_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string> { "123" } // Invalid organization number
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/beslutat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDecided_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        var expectedResponse = new BeslutatGetResponse
        {
            MomsuppgiftBeslut = new Models.DTOs.MomsuppgiftBeslut()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetDecidedDeclarationAsync(redovisare, period))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.GetAsync($"/api/beslutat/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDecided_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var invalidRedovisare = "abc";
        var period = "202401";

        // Act
        var response = await _client.GetAsync($"/api/beslutat/{invalidRedovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
