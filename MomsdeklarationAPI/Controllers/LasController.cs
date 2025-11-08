using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MomsdeklarationAPI.Models.Responses;
using MomsdeklarationAPI.Services;

namespace MomsdeklarationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LasController : ControllerBase
{
    private readonly IMomsdeklarationService _momsdeklarationService;
    private readonly IValidationService _validationService;
    private readonly ILogger<LasController> _logger;

    public LasController(
        IMomsdeklarationService momsdeklarationService,
        IValidationService validationService,
        ILogger<LasController> logger)
    {
        _momsdeklarationService = momsdeklarationService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPut("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(typeof(LasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LockDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod)
    {
        _logger.LogInformation("Locking draft for signing: {Redovisare}/{Redovisningsperiod}", 
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
            var result = await _momsdeklarationService.LockDraftForSigningAsync(
                redovisare, redovisningsperiod);

            if (result)
            {
                return Ok(new LasResponse
                {
                    Last = true,
                    LastTid = DateTime.UtcNow,
                    Status = "LÅST",
                    SigneringsLank = $"https://app.skatteverket.se/signering/momsdeklaration/{redovisare}/{redovisningsperiod}",
                    Meddelande = "Utkastet är låst för signering"
                });
            }

            return Conflict(new ErrorResponse
            {
                Status = 409,
                Error = "Conflict",
                Message = "Draft is already locked or cannot be locked",
                Path = Request.Path
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to lock draft");
            
            if (ex.Message.Contains("404"))
            {
                return NotFound(new ErrorResponse
                {
                    Status = 404,
                    Error = "Not Found",
                    Message = "Draft not found",
                    Path = Request.Path
                });
            }

            if (ex.Message.Contains("409"))
            {
                return Conflict(new ErrorResponse
                {
                    Status = 409,
                    Error = "Conflict",
                    Message = "Draft is already locked by another process",
                    Path = Request.Path
                });
            }

            throw;
        }
    }

    [HttpDelete("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UnlockDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod)
    {
        _logger.LogInformation("Unlocking draft: {Redovisare}/{Redovisningsperiod}", 
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
            var result = await _momsdeklarationService.UnlockDraftAsync(
                redovisare, redovisningsperiod);

            if (result)
            {
                return NoContent();
            }

            return NotFound(new ErrorResponse
            {
                Status = 404,
                Error = "Not Found",
                Message = "Draft not found or not locked",
                Path = Request.Path
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to unlock draft");
            
            if (ex.Message.Contains("404"))
            {
                return NotFound(new ErrorResponse
                {
                    Status = 404,
                    Error = "Not Found",
                    Message = "Draft not found",
                    Path = Request.Path
                });
            }

            throw;
        }
    }
}