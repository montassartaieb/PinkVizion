using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Imaging.API.DTOs;
using PinkVision.Imaging.API.Services;

namespace PinkVision.Imaging.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImagingController : ControllerBase
{
    private readonly IImagingService _imagingService;
    private readonly ILogger<ImagingController> _logger;

    public ImagingController(IImagingService imagingService, ILogger<ImagingController> logger)
    {
        _imagingService = imagingService;
        _logger = logger;
    }

    /// <summary>
    /// Vérifier le statut du service IA
    /// </summary>
    [HttpGet("ai/status")]
    [ProducesResponseType(typeof(ApiResponse<AIServiceStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAIStatus()
    {
        var result = await _imagingService.GetAIServiceStatusAsync();
        return Ok(result);
    }

    /// <summary>
    /// Uploader une image de mammographie
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(ApiResponse<MammographyImageDto>), StatusCodes.Status201Created)]
    [RequestSizeLimit(50_000_000)] // 50 MB max
    public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<MammographyImageDto>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        var result = await _imagingService.UploadImageAsync(request, userId.Value);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetImage), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Analyser une image avec l'IA
    /// </summary>
    [HttpPost("{id:guid}/analyze")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<DiagnosisResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalyzeImage(Guid id, [FromBody] PatientFeaturesDto? features = null)
    {
        var result = await _imagingService.AnalyzeImageAsync(id, features);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer une image par ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MammographyImageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var result = await _imagingService.GetImageByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer les images d'un patient
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<MammographyImageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientImages(Guid patientId)
    {
        var result = await _imagingService.GetImagesByPatientAsync(patientId);
        return Ok(result);
    }

    /// <summary>
    /// Valider un diagnostic (Médecin uniquement)
    /// </summary>
    [HttpPost("diagnosis/validate")]
    [Authorize(Roles = "MEDECIN")]
    [ProducesResponseType(typeof(ApiResponse<DiagnosisResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateDiagnosis([FromBody] ValidateDiagnosisRequest request)
    {
        var doctorId = GetCurrentUserId();
        if (doctorId == null)
        {
            return Unauthorized(new ApiResponse<DiagnosisResultDto>
            {
                Success = false,
                Message = "Médecin non authentifié"
            });
        }

        var result = await _imagingService.ValidateDiagnosisAsync(request, doctorId.Value);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer les diagnostics en attente de validation
    /// </summary>
    [HttpGet("diagnosis/pending")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<List<DiagnosisResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingValidations()
    {
        var result = await _imagingService.GetPendingValidationsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Supprimer une image (Admin uniquement)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var result = await _imagingService.DeleteImageAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
