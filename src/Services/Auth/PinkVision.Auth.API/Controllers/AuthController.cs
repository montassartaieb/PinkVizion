using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Auth.API.DTOs;
using PinkVision.Auth.API.Services;

namespace PinkVision.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Inscription d'un nouvel utilisateur (Patient ou Médecin)
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Données invalides"
            });
        }

        var result = await _authService.RegisterAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Connexion d'un utilisateur
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Données invalides"
            });
        }

        var result = await _authService.LoginAsync(request);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Rafraîchissement du token JWT
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Refresh token requis"
            });
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Déconnexion (révocation du refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest request)
    {
        var result = await _authService.RevokeTokenAsync(request.RefreshToken, "User logout");
        return Ok(result);
    }

    /// <summary>
    /// Changement de mot de passe
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Données invalides"
            });
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new ApiResponse<bool>
            {
                Success = false,
                Message = "Utilisateur non authentifié"
            });
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Vérification de la validité du token
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public IActionResult ValidateToken()
    {
        var userId = GetCurrentUserId();
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Token valide",
            Data = new
            {
                UserId = userId,
                Email = email,
                Roles = roles
            }
        });
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
