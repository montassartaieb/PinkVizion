using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.Doctor.API.Entities;

[Table("doctors")]
public class DoctorEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

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

    [Column("specialty")]
    [MaxLength(100)]
    public string? Specialty { get; set; } // Radiologue, Oncologue, etc.

    [Column("license_number")]
    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    [Column("hospital")]
    [MaxLength(200)]
    public string? Hospital { get; set; }

    [Column("department")]
    [MaxLength(100)]
    public string? Department { get; set; }

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
