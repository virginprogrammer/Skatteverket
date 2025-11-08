using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using MomsdeklarationAPI.Services;

namespace MomsdeklarationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class InlamnatController : ControllerBase
{
    private readonly IInlamnatService _inlamnatService;
    private readonly IValidationService _validationService;
    private readonly ILogger<InlamnatController> _logger;

    public InlamnatController(
        IInlamnatService inlamnatService,
        IValidationService validationService,
        ILogger<InlamnatController> logger)
    {
        _inlamnatService = inlamnatService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InlamnatPostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> GetMultipleSubmitted([FromBody] HamtaPostMultiRequest request)
    {
        _logger.LogInformation("Fetching submitted declarations for {Count} organizations", 
            request?.Redovisare?.Count ?? 0);

        if (request == null || !request.Redovisare.Any())
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = "Invalid request data. Redovisare list is required.",
                Path = Request.Path
            });
        }

        foreach (var redovisare in request.Redovisare)
        {
            var validation = _validationService.ValidateRedovisare(redovisare);
            if (!validation.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Status = 400,
                    Error = "Bad Request",
                    Message = $"Invalid redovisare {redovisare}: {string.Join(", ", validation.Errors)}",
                    Path = Request.Path
                });
            }
        }

        try
        {
            var response = await _inlamnatService.GetMultipleSubmittedAsync(request);
            return Ok(response);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            return NotFound(new ErrorResponse
            {
                Status = 404,
                Error = "Not Found",
                Message = "No submitted declarations found for the specified criteria",
                Path = Request.Path
            });
        }
    }

    [HttpGet("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(typeof(InlamnatGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmitted(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod)
    {
        _logger.LogInformation("Fetching submitted declaration for {Redovisare}/{Redovisningsperiod}", 
            redovisare, redovisningsperiod);

        var redovisareValidation = _validationService.ValidateRedovisare(redovisare);
        if (!redovisareValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", redovisareValidation.Errors),
                Path = Request.Path
            });
        }

        var periodValidation = _validationService.ValidateRedovisningsperiod(redovisningsperiod);
        if (!periodValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", periodValidation.Errors),
                Path = Request.Path
            });
        }

        try
        {
            var response = await _inlamnatService.GetSubmittedAsync(redovisare, redovisningsperiod);
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch submitted declaration");
            
            if (ex.Message.Contains("404"))
            {
                return NotFound(new ErrorResponse
                {
                    Status = 404,
                    Error = "Not Found",
                    Message = "Submitted declaration not found",
                    Path = Request.Path
                });
            }

            throw;
        }
    }
}