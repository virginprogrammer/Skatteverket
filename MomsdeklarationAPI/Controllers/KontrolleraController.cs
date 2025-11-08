using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsdeklarationAPI.Models.DTOs;
using MomsdeklarationAPI.Models.Requests;
using MomsdeklarationAPI.Models.Responses;
using MomsdeklarationAPI.Services;

namespace MomsdeklarationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class KontrolleraController : ControllerBase
{
    private readonly IMomsdeklarationService _momsdeklarationService;
    private readonly IValidationService _validationService;
    private readonly ILogger<KontrolleraController> _logger;

    public KontrolleraController(
        IMomsdeklarationService momsdeklarationService,
        IValidationService validationService,
        ILogger<KontrolleraController> logger)
    {
        _momsdeklarationService = momsdeklarationService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(typeof(KontrollResultat), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> ValidateDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod,
        [FromBody] UtkastPostRequest request)
    {
        _logger.LogInformation("Validating draft for {Redovisare}/{Redovisningsperiod}", 
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

        if (request?.Momsuppgift == null)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = "Momsuppgift is required",
                Path = Request.Path
            });
        }

        try
        {
            var result = await _momsdeklarationService.ValidateDraftAsync(
                redovisare, redovisningsperiod, request);

            if (result.Status == "AVVISAD")
            {
                _logger.LogWarning("Validation failed for {Redovisare}/{Redovisningsperiod}: {Errors}", 
                    redovisare, redovisningsperiod, 
                    string.Join(", ", result.Resultat.Select(r => r.Meddelande)));
            }

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API call failed for validation");
            
            if (ex.Message.Contains("404"))
            {
                return NotFound(new ErrorResponse
                {
                    Status = 404,
                    Error = "Not Found",
                    Message = "Declaration period not found",
                    Path = Request.Path
                });
            }

            throw;
        }
    }
}