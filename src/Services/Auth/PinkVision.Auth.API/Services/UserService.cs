using Microsoft.EntityFrameworkCore;
using PinkVision.Auth.API.Data;
using PinkVision.Auth.API.DTOs;
using PinkVision.Auth.API.Entities;

namespace PinkVision.Auth.API.Services;

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 20);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<ApiResponse<bool>> AssignRoleAsync(AssignRoleRequest request);
    Task<ApiResponse<bool>> RemoveRoleAsync(Guid userId, string roleName);
    Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId);
}

public class UserService : IUserService
{
    private readonly AuthDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AuthDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Erreur lors de la récupération de l'utilisateur"
            };
        }
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ApiResponse<List<UserDto>>
            {
                Success = true,
                Data = users.Select(MapToUserDto).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return new ApiResponse<List<UserDto>>
            {
                Success = false,
                Message = "Erreur lors de la récupération des utilisateurs"
            };
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                };
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone;

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponse<UserDto>
            {
                Success = true,
                Message = "Profil mis à jour avec succès",
                Data = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for {UserId}", userId);
            return new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Erreur lors de la mise à jour du profil"
            };
        }
    }

    public async Task<ApiResponse<bool>> AssignRoleAsync(AssignRoleRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Utilisateur non trouvé",
                    Data = false
                };
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName.ToUpper());
            if (role == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Rôle non trouvé",
                    Data = false
                };
            }

            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == role.Id);

            if (existingUserRole != null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "L'utilisateur a déjà ce rôle",
                    Data = false
                };
            }

            var userRole = new UserRole
            {
                UserId = request.UserId,
                RoleId = role.Id,
                AssignedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role {Role} assigned to user {UserId}", request.RoleName, request.UserId);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Rôle assigné avec succès",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserId}", request.UserId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de l'assignation du rôle",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> RemoveRoleAsync(Guid userId, string roleName)
    {
        try
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName.ToUpper());
            if (role == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Rôle non trouvé",
                    Data = false
                };
            }

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "L'utilisateur n'a pas ce rôle",
                    Data = false
                };
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Rôle retiré avec succès",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors du retrait du rôle",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> DeactivateUserAsync(Guid userId)
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

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke all refresh tokens
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "User deactivated";
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Utilisateur désactivé",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la désactivation",
                Data = false
            };
        }
    }

    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid userId)
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

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Utilisateur réactivé",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", userId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Erreur lors de la réactivation",
                Data = false
            };
        }
    }

    private static UserDto MapToUserDto(User user)
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
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
