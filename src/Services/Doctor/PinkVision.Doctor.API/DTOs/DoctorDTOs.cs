using System.ComponentModel.DataAnnotations;

namespace PinkVision.Doctor.API.DTOs;

public class CreateDoctorRequest
{
    [Required]
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Hospital { get; set; }
    public string? Department { get; set; }
}

public class UpdateDoctorRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Specialty { get; set; }
    public string? Hospital { get; set; }
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public bool? IsAvailable { get; set; }
}

public class DoctorDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"Dr. {FirstName} {LastName}";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Hospital { get; set; }
    public string? Department { get; set; }
    public string? Bio { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
