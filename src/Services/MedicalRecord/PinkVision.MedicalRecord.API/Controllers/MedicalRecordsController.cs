using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.MedicalRecord.API.DTOs;
using PinkVision.MedicalRecord.API.Services;

namespace PinkVision.MedicalRecord.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _service;

    public MedicalRecordsController(IMedicalRecordService service)
    {
        _service = service;
    }

    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "MEDECIN,ADMIN,PATIENT")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        // Security check: Patients can only access their own record
        if (User.IsInRole("PATIENT"))
        {
            var userId = GetCurrentUserId();
            if (userId != patientId) return Forbid();
        }

        var result = await _service.GetByPatientIdAsync(patientId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("patient/{patientId:guid}")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    public async Task<IActionResult> CreateOrUpdate(Guid patientId, [FromBody] CreateOrUpdateRecordRequest request)
    {
        var result = await _service.CreateOrUpdateAsync(patientId, request);
        return Ok(result);
    }

    [HttpPost("patient/{patientId:guid}/entries")]
    [Authorize(Roles = "MEDECIN")]
    public async Task<IActionResult> AddEntry(Guid patientId, [FromBody] AddEntryRequest request)
    {
        var result = await _service.AddEntryAsync(patientId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
