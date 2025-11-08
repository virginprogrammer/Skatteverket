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
public class UtkastController : ControllerBase
{
    private readonly IMomsdeklarationService _momsdeklarationService;
    private readonly IValidationService _validationService;
    private readonly ILogger<UtkastController> _logger;

    public UtkastController(
        IMomsdeklarationService momsdeklarationService,
        IValidationService validationService,
        ILogger<UtkastController> logger)
    {
        _momsdeklarationService = momsdeklarationService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UtkastPostMultiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMultipleDrafts([FromBody] HamtaPostMultiRequest request)
    {
        if (request == null || !request.Redovisare.Any())
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = "Invalid request data"
            });
        }

        var response = await _momsdeklarationService.GetMultipleDraftsAsync(request);
        return Ok(response);
    }

    [HttpPost("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(typeof(UtkastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrUpdateDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod,
        [FromBody] UtkastPostRequest request,
        [FromQuery] bool lock_ = false)
    {
        var redovisareValidation = _validationService.ValidateRedovisare(redovisare);
        if (!redovisareValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", redovisareValidation.Errors)
            });
        }

        var periodValidation = _validationService.ValidateRedovisningsperiod(redovisningsperiod);
        if (!periodValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", periodValidation.Errors)
            });
        }

        var response = await _momsdeklarationService.CreateOrUpdateDraftAsync(
            redovisare, redovisningsperiod, request, lock_);
        
        return Ok(response);
    }

    [HttpGet("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(typeof(UtkastGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod)
    {
        var redovisareValidation = _validationService.ValidateRedovisare(redovisare);
        if (!redovisareValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", redovisareValidation.Errors)
            });
        }

        var periodValidation = _validationService.ValidateRedovisningsperiod(redovisningsperiod);
        if (!periodValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", periodValidation.Errors)
            });
        }

        var response = await _momsdeklarationService.GetDraftAsync(redovisare, redovisningsperiod);
        return Ok(response);
    }

    [HttpDelete("{redovisare}/{redovisningsperiod}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDraft(
        [FromRoute] string redovisare,
        [FromRoute] string redovisningsperiod)
    {
        var redovisareValidation = _validationService.ValidateRedovisare(redovisare);
        if (!redovisareValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", redovisareValidation.Errors)
            });
        }

        var periodValidation = _validationService.ValidateRedovisningsperiod(redovisningsperiod);
        if (!periodValidation.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Error = "Bad Request",
                Message = string.Join(", ", periodValidation.Errors)
            });
        }

        var result = await _momsdeklarationService.DeleteDraftAsync(redovisare, redovisningsperiod);
        
        if (result)
        {
            return NoContent();
        }

        return NotFound(new ErrorResponse
        {
            Status = 404,
            Error = "Not Found",
            Message = "Draft not found"
        });
    }
}