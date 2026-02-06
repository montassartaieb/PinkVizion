using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.MedicalRecord.API.Entities;

[Table("medical_records")]
public class MedicalRecordEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("patient_id")]
    public Guid PatientId { get; set; }

    [Column("allergies")]
    public List<string> Allergies { get; set; } = new();

    [Column("chronic_conditions")]
    public List<string> ChronicConditions { get; set; } = new();

    [Column("treatments")]
    public List<string> Treatments { get; set; } = new();

    [Column("family_history")]
    public string? FamilyHistory { get; set; } // Antécédents familiaux (cancer du sein, etc.)

    [Column("blood_type")]
    [MaxLength(5)]
    public string? BloodType { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for entries (consultations, prescriptions) could be added here
    public virtual ICollection<MedicalEntry> Entries { get; set; } = new List<MedicalEntry>();
}

[Table("medical_entries")]
public class MedicalEntry
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("medical_record_id")]
    public Guid MedicalRecordId { get; set; }

    [Column("doctor_id")]
    public Guid DoctorId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column("type")]
    [MaxLength(50)]
    public string Type { get; set; } = "CONSULTATION"; // CONSULTATION, PRESCRIPTION, EXAM, SURGERY

    [Column("description")]
    public string? Description { get; set; }

    [Column("attachment_urls")]
    public List<string> AttachmentUrls { get; set; } = new();

    public virtual MedicalRecordEntity MedicalRecord { get; set; } = null!;
}
