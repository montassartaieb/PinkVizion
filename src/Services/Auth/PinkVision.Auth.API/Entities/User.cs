using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PinkVision.Auth.API.Entities;

/// <summary>
/// Représente un utilisateur dans le système
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("first_name")]
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [MaxLength(100)]
    public string? LastName { get; set; }

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("email_confirmed")]
    public bool EmailConfirmed { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

/// <summary>
/// Représente un rôle dans le système (ADMIN, MEDECIN, PATIENT)
/// </summary>
[Table("roles")]
public class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(255)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

/// <summary>
/// Table de liaison User-Role (many-to-many)
/// </summary>
[Table("user_roles")]
public class UserRole
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

/// <summary>
/// Représente un token de rafraîchissement
/// </summary>
[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("replaced_by_token")]
    public string? ReplacedByToken { get; set; }

    [Column("revoked_reason")]
    [MaxLength(255)]
    public string? RevokedReason { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
