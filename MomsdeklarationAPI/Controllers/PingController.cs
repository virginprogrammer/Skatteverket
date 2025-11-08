using Microsoft.AspNetCore.Mvc;
using MomsdeklarationAPI.Models.Responses;

namespace MomsdeklarationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PingResponse), StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint called");
        
        return Ok(new PingResponse
        {
            Status = "OK",
            Timestamp = DateTime.UtcNow,
            Service = "MomsdeklarationAPI",
            Version = "1.0.0"
        });
    }
}

public class PingResponse
{
    public string Status { get; set; } = "OK";
    public DateTime Timestamp { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}