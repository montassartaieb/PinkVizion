using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Appointment.API.DTOs;
using PinkVision.Appointment.API.Services;

namespace PinkVision.Appointment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId)
    {
        var result = await _appointmentService.GetByPatientAsync(patientId);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId, [FromQuery] DateTime? date = null)
    {
        var result = await _appointmentService.GetByDoctorAsync(doctorId, date);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var isDoctor = User.IsInRole("MEDECIN");
        var result = await _appointmentService.GetUpcomingAsync(userId.Value, isDoctor);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        var result = await _appointmentService.CreateAsync(request);
        return result.Success 
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) 
            : BadRequest(result);
    }

    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = "MEDECIN,ADMIN")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _appointmentService.ConfirmAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        var result = await _appointmentService.CancelAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = "MEDECIN")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] string? notes = null)
    {
        var result = await _appointmentService.CompleteAsync(id, notes);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
