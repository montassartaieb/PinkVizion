using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Patient.API.DTOs;
using PinkVision.Patient.API.Services;

namespace PinkVision.Patient.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer le profil du patient connecté
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "PATIENT")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        var result = await _patientService.GetByUserIdAsync(userId.Value);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mettre à jour le profil du patient connecté
    /// </summary>
    [HttpPut("me")]
    [Authorize(Roles = "PATIENT")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdatePatientRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<PatientDto>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        // Get patient by user ID first
        var patientResult = await _patientService.GetByUserIdAsync(userId.Value);
        if (!patientResult.Success || patientResult.Data == null)
        {
            return NotFound(patientResult);
        }

        var result = await _patientService.UpdateAsync(patientResult.Data.Id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer un patient par ID (Médecin/Admin)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _patientService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer tous les patients avec pagination (Médecin/Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _patientService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>
    /// Créer un nouveau patient
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        // If current user is a patient, set their own userId
        var currentUserId = GetCurrentUserId();
        if (User.IsInRole("PATIENT") && currentUserId.HasValue)
        {
            request.UserId = currentUserId.Value;
        }

        var result = await _patientService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Mettre à jour un patient (Médecin/Admin)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request)
    {
        var result = await _patientService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Supprimer un patient (Admin uniquement)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _patientService.DeleteAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer la liste des groupes sanguins
    /// </summary>
    [HttpGet("blood-groups")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<BloodGroupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBloodGroups()
    {
        var result = await _patientService.GetBloodGroupsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Enregistrer les mesures vitales d'un patient (Médecin)
    /// </summary>
    [HttpPost("{id:guid}/vitals")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    [ProducesResponseType(typeof(ApiResponse<VitalsHistoryDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordVitals(Guid id, [FromBody] RecordVitalsRequest request)
    {
        var doctorId = GetCurrentUserId();
        var result = await _patientService.RecordVitalsAsync(id, request, doctorId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Created("", result);
    }

    /// <summary>
    /// Récupérer l'historique des mesures vitales d'un patient
    /// </summary>
    [HttpGet("{id:guid}/vitals")]
    [ProducesResponseType(typeof(ApiResponse<List<VitalsHistoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVitalsHistory(Guid id, [FromQuery] int limit = 10)
    {
        // Patients can only see their own vitals
        if (User.IsInRole("PATIENT"))
        {
            var userId = GetCurrentUserId();
            var patientResult = await _patientService.GetByUserIdAsync(userId!.Value);
            if (!patientResult.Success || patientResult.Data?.Id != id)
            {
                return Forbid();
            }
        }

        var result = await _patientService.GetVitalsHistoryAsync(id, limit);
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
