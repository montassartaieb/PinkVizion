using System.ComponentModel.DataAnnotations;

namespace PinkVision.MedicalRecord.API.DTOs;

public class CreateOrUpdateRecordRequest
{
    public List<string>? Allergies { get; set; }
    public List<string>? ChronicConditions { get; set; }
    public List<string>? Treatments { get; set; }
    public string? FamilyHistory { get; set; }
    public string? BloodType { get; set; }
}

public class AddEntryRequest
{
    [Required]
    public Guid DoctorId { get; set; }
    [Required]
    public string Type { get; set; } = "CONSULTATION";
    public string? Description { get; set; }
    public List<string>? AttachmentUrls { get; set; }
}

public class MedicalRecordDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public List<string> Allergies { get; set; } = new();
    public List<string> ChronicConditions { get; set; } = new();
    public List<string> Treatments { get; set; } = new();
    public string? FamilyHistory { get; set; }
    public string? BloodType { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MedicalEntryDto> Entries { get; set; } = new();
}

public class MedicalEntryDto
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> AttachmentUrls { get; set; } = new();
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
