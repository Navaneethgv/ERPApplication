using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Employee")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] int limit = 20)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Employee";
        var notifications = _notificationService.GetAll(role, limit);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public IActionResult GetUnreadCount()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Employee";
        var count = _notificationService.GetUnreadCount(role);
        return Ok(new UnreadCountDto { UnreadCount = count });
    }

    [HttpPut("{id}/read")]
    public IActionResult MarkAsRead(int id)
    {
        var success = _notificationService.MarkAsRead(id);
        if (!success)
        {
            return NotFound(new { message = $"Notification with ID {id} not found." });
        }
        return Ok(new { success = true });
    }

    [HttpPut("mark-all-read")]
    public IActionResult MarkAllAsRead()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Employee";
        var count = _notificationService.MarkAllAsRead(role);
        return Ok(new { updatedCount = count });
    }
}

