using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Doctor.API.DTOs;
using PinkVision.Doctor.API.Services;

namespace PinkVision.Doctor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet("me")]
    [Authorize(Roles = "MEDECIN")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _doctorService.GetByUserIdAsync(userId.Value);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("me")]
    [Authorize(Roles = "MEDECIN")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var doctorResult = await _doctorService.GetByUserIdAsync(userId.Value);
        if (!doctorResult.Success) return NotFound(doctorResult);

        var result = await _doctorService.UpdateAsync(doctorResult.Data!.Id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? specialty = null)
    {
        var result = await _doctorService.GetAllAsync(page, pageSize, specialty);
        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _doctorService.GetAvailableDoctorsAsync();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
