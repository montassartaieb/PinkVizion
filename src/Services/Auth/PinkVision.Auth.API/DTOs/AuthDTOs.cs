using System.ComponentModel.DataAnnotations;

namespace PinkVision.Auth.API.DTOs;

// ============================================
// REQUEST DTOs
// ============================================

/// <summary>
/// DTO pour l'inscription d'un nouvel utilisateur
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est requis")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Format de téléphone invalide")]
    public string? Phone { get; set; }

    /// <summary>
    /// Type d'utilisateur: PATIENT ou MEDECIN
    /// </summary>
    [Required(ErrorMessage = "Le type d'utilisateur est requis")]
    public string UserType { get; set; } = "PATIENT";
}

/// <summary>
/// DTO pour la connexion
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour le rafraîchissement du token
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Le refresh token est requis")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la révocation du token
/// </summary>
public class RevokeTokenRequest
{
    [Required(ErrorMessage = "Le refresh token est requis")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour le changement de mot de passe
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Le mot de passe actuel est requis")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// ============================================
// RESPONSE DTOs
// ============================================

/// <summary>
/// DTO de réponse d'authentification
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
}

/// <summary>
/// DTO représentant un utilisateur
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// DTO de réponse API standard
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO pour la mise à jour du profil utilisateur
/// </summary>
public class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "Format de téléphone invalide")]
    public string? Phone { get; set; }
}

/// <summary>
/// DTO pour l'assignation d'un rôle à un utilisateur (Admin only)
/// </summary>
public class AssignRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string RoleName { get; set; } = string.Empty;
}
