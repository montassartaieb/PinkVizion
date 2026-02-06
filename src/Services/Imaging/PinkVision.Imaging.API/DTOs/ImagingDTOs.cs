using System.ComponentModel.DataAnnotations;

namespace PinkVision.Imaging.API.DTOs;

// ============================================
// REQUEST DTOs
// ============================================

public class UploadImageRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public Guid PatientId { get; set; }

    [MaxLength(50)]
    public string? ImageType { get; set; } // LEFT, RIGHT

    [MaxLength(50)]
    public string? ViewPosition { get; set; } // CC, MLO

    public string? Notes { get; set; }
}

public class AnalyzeImageRequest
{
    [Required]
    public Guid ImageId { get; set; }

    /// <summary>
    /// Features optionnelles pour le modèle tabulaire
    /// </summary>
    public PatientFeaturesDto? PatientFeatures { get; set; }
}

public class PatientFeaturesDto
{
    public int? Age { get; set; }
    public string? Menopause { get; set; } // premeno, ge40, lt40
    public int? TumorSize { get; set; }
    public string? InvNodes { get; set; }
    public int? NodeCaps { get; set; }
    public int? DegMalig { get; set; }
    public string? Breast { get; set; } // left, right
    public string? BreastQuad { get; set; }
    public string? Irradiat { get; set; } // yes, no
}

public class ValidateDiagnosisRequest
{
    [Required]
    public Guid DiagnosisId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FinalDiagnosis { get; set; } = string.Empty; // BENIGN, MALIGNANT, UNCERTAIN

    public string? DoctorNotes { get; set; }
}

// ============================================
// RESPONSE DTOs
// ============================================

public class MammographyImageDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ContentType { get; set; }
    public string? ImageType { get; set; }
    public string? ViewPosition { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? AnalyzedAt { get; set; }
    public DiagnosisResultDto? DiagnosisResult { get; set; }
}

public class DiagnosisResultDto
{
    public Guid Id { get; set; }
    public Guid ImageId { get; set; }
    public Guid PatientId { get; set; }
    public string Label { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string ProbabilityPercent => $"{Probability * 100:F1}%";
    public double? PImage { get; set; }
    public double? PTabular { get; set; }
    public string? ModelVersion { get; set; }
    public bool DegradedMode { get; set; }
    public string? HeatmapPath { get; set; }
    public bool DoctorValidated { get; set; }
    public Guid? ValidatedByDoctorId { get; set; }
    public string? DoctorNotes { get; set; }
    public string? FinalDiagnosis { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    
    // Risk level based on probability
    public string RiskLevel => Probability switch
    {
        >= 0.75 => "HIGH",
        >= 0.5 => "MODERATE",
        >= 0.25 => "LOW",
        _ => "VERY_LOW"
    };
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class AIServiceStatusDto
{
    public bool Available { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool ImageModelLoaded { get; set; }
    public bool TabularModelLoaded { get; set; }
    public string? ImageModelError { get; set; }
    public string? TabularModelError { get; set; }
}
