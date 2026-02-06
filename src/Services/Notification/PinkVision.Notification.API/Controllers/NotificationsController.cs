using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PinkVision.Notification.API.Services;

namespace PinkVision.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.GetByUserAsync(userId, unreadOnly);
        return Ok(result);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _notificationService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    }
}
