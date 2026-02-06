using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.Appointment.API.Entities;

[Table("appointments")]
public class AppointmentEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("patient_id")]
    public Guid PatientId { get; set; }

    [Required]
    [Column("doctor_id")]
    public Guid DoctorId { get; set; }

    [Required]
    [Column("scheduled_at")]
    public DateTime ScheduledAt { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; } = 30;

    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "PENDING"; // PENDING, CONFIRMED, CANCELLED, COMPLETED

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancellation_reason")]
    public string? CancellationReason { get; set; }
}
