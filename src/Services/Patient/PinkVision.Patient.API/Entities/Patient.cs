using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.Patient.API.Entities;

/// <summary>
/// Représente un patient dans le système
/// </summary>
[Table("patients")]
public class PatientEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID de l'utilisateur associé (from Auth Service)
    /// </summary>
    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("first_name")]
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [MaxLength(100)]
    public string? LastName { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Âge (calculé ou saisi directement)
    /// </summary>
    [Column("age")]
    public int? Age { get; set; }

    [Column("gender")]
    [MaxLength(20)]
    public string? Gender { get; set; }

    [Column("blood_group_id")]
    public int? BloodGroupId { get; set; }

    [Column("weight_kg")]
    public decimal? WeightKg { get; set; }

    [Column("height_cm")]
    public decimal? HeightCm { get; set; }

    [Column("address")]
    [MaxLength(500)]
    public string? Address { get; set; }

    [Column("city")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("emergency_contact_name")]
    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [Column("emergency_contact_phone")]
    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    /// <summary>
    /// Maladie / Suivi
    /// </summary>
    [Column("disease_followup")]
    public string? DiseaseFollowup { get; set; }

    [Column("allergies")]
    public string? Allergies { get; set; }

    [Column("medical_history")]
    public string? MedicalHistory { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual BloodGroup? BloodGroup { get; set; }
    public virtual ICollection<VitalsHistory> VitalsHistory { get; set; } = new List<VitalsHistory>();
}

/// <summary>
/// Lookup table pour les groupes sanguins
/// </summary>
[Table("blood_groups")]
public class BloodGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("code")]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(50)]
    public string? Description { get; set; }
}

/// <summary>
/// Historique des mesures vitales
/// </summary>
[Table("vitals_history")]
public class VitalsHistory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("patient_id")]
    public Guid PatientId { get; set; }

    [Column("measured_at")]
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    [Column("weight_kg")]
    public decimal? WeightKg { get; set; }

    [Column("height_cm")]
    public decimal? HeightCm { get; set; }

    [Column("blood_pressure_systolic")]
    public int? BloodPressureSystolic { get; set; }

    [Column("blood_pressure_diastolic")]
    public int? BloodPressureDiastolic { get; set; }

    [Column("heart_rate")]
    public int? HeartRate { get; set; }

    [Column("temperature")]
    public decimal? Temperature { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("recorded_by_doctor_id")]
    public Guid? RecordedByDoctorId { get; set; }

    // Navigation property
    public virtual PatientEntity Patient { get; set; } = null!;
}
