using System.ComponentModel.DataAnnotations;

namespace PinkVision.Patient.API.DTOs;

// ============================================
// REQUEST DTOs
// ============================================

public class CreatePatientRequest
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? Age { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public int? BloodGroupId { get; set; }

    [Range(0, 500)]
    public decimal? WeightKg { get; set; }

    [Range(0, 300)]
    public decimal? HeightCm { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public string? DiseaseFollowup { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; }
}

public class UpdatePatientRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? Age { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    public int? BloodGroupId { get; set; }

    [Range(0, 500)]
    public decimal? WeightKg { get; set; }

    [Range(0, 300)]
    public decimal? HeightCm { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    public string? DiseaseFollowup { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; }
}

public class RecordVitalsRequest
{
    [Range(0, 500)]
    public decimal? WeightKg { get; set; }

    [Range(0, 300)]
    public decimal? HeightCm { get; set; }

    [Range(0, 300)]
    public int? BloodPressureSystolic { get; set; }

    [Range(0, 200)]
    public int? BloodPressureDiastolic { get; set; }

    [Range(0, 300)]
    public int? HeartRate { get; set; }

    [Range(30, 45)]
    public decimal? Temperature { get; set; }

    public string? Notes { get; set; }
}

// ============================================
// RESPONSE DTOs
// ============================================

public class PatientDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public BloodGroupDto? BloodGroup { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? BMI => (WeightKg.HasValue && HeightCm.HasValue && HeightCm > 0) 
        ? Math.Round(WeightKg.Value / ((HeightCm.Value / 100) * (HeightCm.Value / 100)), 2) 
        : null;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? DiseaseFollowup { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BloodGroupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class VitalsHistoryDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public string? BloodPressure => (BloodPressureSystolic.HasValue && BloodPressureDiastolic.HasValue)
        ? $"{BloodPressureSystolic}/{BloodPressureDiastolic}"
        : null;
    public int? HeartRate { get; set; }
    public decimal? Temperature { get; set; }
    public string? Notes { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
