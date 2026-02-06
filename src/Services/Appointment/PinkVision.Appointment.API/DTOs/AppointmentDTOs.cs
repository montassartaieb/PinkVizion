using System.ComponentModel.DataAnnotations;

namespace PinkVision.Appointment.API.DTOs;

public class CreateAppointmentRequest
{
    [Required]
    public Guid PatientId { get; set; }
    [Required]
    public Guid DoctorId { get; set; }
    [Required]
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? Reason { get; set; }
}

public class UpdateAppointmentRequest
{
    public DateTime? ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
}

public class CancelAppointmentRequest
{
    public string? CancellationReason { get; set; }
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
