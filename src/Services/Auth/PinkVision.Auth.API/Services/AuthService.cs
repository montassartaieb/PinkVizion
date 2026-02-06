using Microsoft.EntityFrameworkCore;
using PinkVision.Auth.API.Data;
using PinkVision.Auth.API.DTOs;
using PinkVision.Auth.API.Entities;
using MassTransit;

namespace PinkVision.Auth.API.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken, string reason = "User logout");
    Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AuthDbContext context,
        ITokenService tokenService,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Un utilisateur avec cet email existe déjà"
                };
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);

            // Assign role based on UserType
            var roleName = request.UserType.ToUpper() switch
            {
                "MEDECIN" => "MEDECIN",
                "DOCTOR" => "MEDECIN",
                _ => "PATIENT"
            };

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                };
                await _context.UserRoles.AddAsync(userRole);
            }

            await _context.SaveChangesAsync();

            // Generate tokens
            var roles = new List<string> { roleName };
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Publish user registered event
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = roleName,
                RegisteredAt = DateTime.UtcNow
            });

            _logger.LogInformation("User registered successfully: {Email}", user.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "Inscription réussie",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapToUserDto(user, roles)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Une erreur est survenue lors de l'inscription"
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Ce compte a été désactivé"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate tokens
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {Email}", user.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "Connexion réussie",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapToUserDto(user, roles)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Une erreur est survenue lors de la connexion"
            };
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token invalide"
                };
            }

            if (!token.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Token expiré ou révoqué"
                };
            }

            var user = token.User;
            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Compte désactivé"
                };
            }

            // Revoke old token
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Token refresh";

            // Generate new tokens
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

            token.ReplacedByToken = newRefreshToken.Token;
            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Token rafraîchi",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapToUserDto(user, roles)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponse
            {
                Success = false,
                Message = "Une erreur est survenue"
            };
        }
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken, string reason = "User logout")
    {
        try
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Token non trouvé",
                    Data = false
                };
            }

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Token révoqué avec succès",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la révocation",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé",
                    Data = false
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Mot de passe actuel incorrect",
                    Data = false
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all refresh tokens
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "Password changed";
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Mot de passe modifié avec succès",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors du changement de mot de passe",
                Data = false
            };
        }
    }

    private static UserDto MapToUserDto(User user, IEnumerable<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}

// Event for MassTransit
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string UserType { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}
