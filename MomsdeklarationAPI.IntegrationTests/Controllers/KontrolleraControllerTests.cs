using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MomsdeklarationAPI.Models.DTOs;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using Moq;
using Xunit;

namespace MomsdeklarationAPI.IntegrationTests.Controllers;

public class KontrolleraControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public KontrolleraControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    }

    [Fact]
    public async Task ValidateDraft_WithValidData_ReturnsOk()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";
        var request = new UtkastPostRequest
        {
            Momsuppgift = new Momsuppgift()
        };

        var expectedResponse = new KontrollResultat
        {
            Resultat = "VALID",
            Felmeddelanden = new List<string>()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.ValidateDraftAsync(redovisare, period, It.IsAny<UtkastPostRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/kontrollera/{redovisare}/{period}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<KontrollResultat>();
        result.Should().NotBeNull();
        result!.Resultat.Should().Be("VALID");
    }

    [Fact]
    public async Task ValidateDraft_WithInvalidRedovisare_ReturnsBadRequest()
    {
        // Arrange
        var invalidRedovisare = "123";
        var period = "202401";
        var request = new UtkastPostRequest
        {
            Momsuppgift = new Momsuppgift()
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/kontrollera/{invalidRedovisare}/{period}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateDraft_WithInvalidPeriod_ReturnsBadRequest()
    {
        // Arrange
        var redovisare = "5555555555";
        var invalidPeriod = "invalid";
        var request = new UtkastPostRequest
        {
            Momsuppgift = new Momsuppgift()
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/kontrollera/{redovisare}/{invalidPeriod}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateDraft_ReturnsJsonContentType()
    {
        // Arrange
        var redovisare = "5555555555";
        var period = "202401";
        var request = new UtkastPostRequest
        {
            Momsuppgift = new Momsuppgift()
        };

        var expectedResponse = new KontrollResultat
        {
            Resultat = "VALID",
            Felmeddelanden = new List<string>()
        };

        _factory.MockSkatteverketApiClient
            .Setup(x => x.ValidateDraftAsync(redovisare, period, It.IsAny<UtkastPostRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/kontrollera/{redovisare}/{period}", request);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
