using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.Imaging.API.Entities;

/// <summary>
/// Représente une image de mammographie uploadée
/// </summary>
[Table("mammography_images")]
public class MammographyImage
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("patient_id")]
    public Guid PatientId { get; set; }

    [Column("uploaded_by_user_id")]
    public Guid? UploadedByUserId { get; set; }

    [Required]
    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [Column("original_file_name")]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Column("file_path")]
    [MaxLength(500)]
    public string? FilePath { get; set; }

    [Column("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [Column("content_type")]
    [MaxLength(100)]
    public string? ContentType { get; set; }

    [Column("image_type")]
    [MaxLength(50)]
    public string? ImageType { get; set; } // LEFT, RIGHT, CC, MLO

    [Column("view_position")]
    [MaxLength(50)]
    public string? ViewPosition { get; set; } // Craniocaudal, Mediolateral

    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "PENDING"; // PENDING, ANALYZING, ANALYZED, FAILED

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("analyzed_at")]
    public DateTime? AnalyzedAt { get; set; }

    // Navigation property
    public virtual DiagnosisResult? DiagnosisResult { get; set; }
}

/// <summary>
/// Résultat du diagnostic IA pour une mammographie
/// </summary>
[Table("diagnosis_results")]
public class DiagnosisResult
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("image_id")]
    public Guid ImageId { get; set; }

    [Column("patient_id")]
    public Guid PatientId { get; set; }

    [Required]
    [Column("label")]
    [MaxLength(50)]
    public string Label { get; set; } = string.Empty; // BENIGN, MALIGNANT

    [Column("probability")]
    public double Probability { get; set; }

    [Column("p_image")]
    public double? PImage { get; set; }

    [Column("p_tabular")]
    public double? PTabular { get; set; }

    [Column("model_version")]
    [MaxLength(50)]
    public string? ModelVersion { get; set; }

    [Column("degraded_mode")]
    public bool DegradedMode { get; set; }

    [Column("heatmap_path")]
    [MaxLength(500)]
    public string? HeatmapPath { get; set; }

    [Column("doctor_validated")]
    public bool DoctorValidated { get; set; }

    [Column("validated_by_doctor_id")]
    public Guid? ValidatedByDoctorId { get; set; }

    [Column("doctor_notes")]
    public string? DoctorNotes { get; set; }

    [Column("final_diagnosis")]
    [MaxLength(50)]
    public string? FinalDiagnosis { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("validated_at")]
    public DateTime? ValidatedAt { get; set; }

    // Navigation property
    public virtual MammographyImage Image { get; set; } = null!;
}
