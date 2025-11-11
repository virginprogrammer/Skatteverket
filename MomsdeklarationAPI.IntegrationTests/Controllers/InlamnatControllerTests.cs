using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Moq;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class InlamnatControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public InlamnatControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task GetMultipleSubmitted_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string> { "5555555555", "1234567890" }
        };

        var expectedResponse = new InlamnatPostResponse
        {
            Inlamnade = new List<InlamnatItem>()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetMultipleSubmittedDeclarationsAsync(It.IsAny<HamtaPostMultiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync("/api/inlamnat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMultipleSubmitted_WithEmptyRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inlamnat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Message.Should().Contain("Invalid request data");
    }

    [Fact]
    public async Task GetMultipleSubmitted_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var request = new HamtaPostMultiRequest
        {
            Redovisare = new List<string> { "invalid" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/inlamnat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSubmitted_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";

        var expectedResponse = new InlamnatGetResponse
        {
            Momsuppgift = new Models.DTOs.Momsuppgift()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.GetSubmittedDeclarationAsync(redovisare, period))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.GetAsync($"/api/inlamnat/{redovisare}/{period}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSubmitted_WithInvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        var redovisare = "5555555555";
        var invalidPeriod = "2024"; // Invalid period format

        // Act
        var response = await _client.GetAsync($"/api/inlamnat/{redovisare}/{invalidPeriod}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
