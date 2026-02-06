using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Auth.API.DTOs;
using PinkVision.Auth.API.Services;

namespace PinkVision.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer le profil de l'utilisateur connecté
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        var result = await _userService.GetUserByIdAsync(userId.Value);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Mettre à jour le profil de l'utilisateur connecté
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        var result = await _userService.UpdateProfileAsync(userId.Value, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer un utilisateur par ID (Admin/Médecin)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "DoctorOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Récupérer tous les utilisateurs (Admin uniquement)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetAllUsersAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Assigner un rôle à un utilisateur (Admin uniquement)
    /// </summary>
    [HttpPost("assign-role")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var result = await _userService.AssignRoleAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retirer un rôle à un utilisateur (Admin uniquement)
    /// </summary>
    [HttpDelete("{userId:guid}/roles/{roleName}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRole(Guid userId, string roleName)
    {
        var result = await _userService.RemoveRoleAsync(userId, roleName);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Désactiver un utilisateur (Admin uniquement)
    /// </summary>
    [HttpPost("{userId:guid}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        var result = await _userService.DeactivateUserAsync(userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Réactiver un utilisateur (Admin uniquement)
    /// </summary>
    [HttpPost("{userId:guid}/activate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        var result = await _userService.ActivateUserAsync(userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        
        return null;
    }
}
